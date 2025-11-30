use core::ops::Index;

use lizstate_crawler::commands::CommandTrait;
use lizstate_crawler::commands::StepMacro;
use lizstate_crawler::entity::Entity;
use lizstate_crawler::floor::Floor;
use lizstate_crawler::spatial::relative::KingStep;
use ratatui::crossterm::event::KeyModifiers;
use ratatui::crossterm::event::{self};
use ratatui::prelude::*;
use ratatui::widgets::*;

use crate::FORCE_CLOSE;
use crate::app::AppState;
use crate::camera::Camera;

pub struct FloorWidget<'a> {
    floor: &'a Floor,
    look_at: (i32, i32),
}

pub struct FloorState {
    floor: Floor,
    look_at: (i32, i32),
}

impl FloorState {
    pub fn new_test() -> Self {
        Self {
            floor: Floor::new_test(),
            look_at: (0, 0),
        }
    }
}

impl AppState for FloorState {
    fn render(&self, frame: &mut Frame, area: Rect) {
        let widget = FloorWidget {
            floor: &self.floor,
            look_at: self.look_at,
        };

        frame.render_widget(widget, area);
    }

    fn handle_events(&mut self) -> std::io::Result<()> {
        if let event::Event::Key(key_event) = event::read()? {
            self.handle_key_event(key_event)
        };
        Ok(())
    }
}

impl FloorState {
    fn handle_key_event(&mut self, key_event: event::KeyEvent) {
        if key_event.kind != event::KeyEventKind::Press {
            return;
        }

        if key_event.modifiers.contains(KeyModifiers::CONTROL) {
            #[allow(clippy::single_match)]
            match key_event.code {
                event::KeyCode::Char('c') => unsafe { FORCE_CLOSE = true },
                _ => {}
            }
        }

        let command = match key_event.code {
            event::KeyCode::Char('.') => Some(StepMacro(None)),
            event::KeyCode::Char('h') => Some(StepMacro(Some(KingStep::West))),
            event::KeyCode::Char('j') => Some(StepMacro(Some(KingStep::South))),
            event::KeyCode::Char('k') => Some(StepMacro(Some(KingStep::North))),
            event::KeyCode::Char('l') => Some(StepMacro(Some(KingStep::East))),
            event::KeyCode::Char('y') => Some(StepMacro(Some(KingStep::NorthWest))),
            event::KeyCode::Char('u') => Some(StepMacro(Some(KingStep::NorthEast))),
            event::KeyCode::Char('b') => Some(StepMacro(Some(KingStep::SouthWest))),
            event::KeyCode::Char('n') => Some(StepMacro(Some(KingStep::SouthEast))),
            _ => None,
        };

        if let Some(command) = command {
            let result = command.do_command(
                &self
                    .floor
                    .try_into_turntaker()
                    .expect("ASSUMPTION: Game does not end"),
            );
            if let Ok(floor) = result {
                self.floor = floor;

                // Painpoint: This could give the player control of a creature that isn't the player.
                for _ in 0..100 {
                    let turntaker = self
                        .floor
                        .try_into_turntaker()
                        .expect("ASSUMPTION: Game does not end.");

                    if let Some(turn_taken) = turntaker.take_turn_if_not_player(0) {
                        self.floor = turn_taken;
                    } else {
                        break;
                    }
                }
            }
        }
    }
}

impl<'a> Widget for FloorWidget<'a> {
    fn render(self, area: Rect, buf: &mut Buffer)
    where
        Self: Sized,
    {
        let block = Block::bordered()
            .title(" dungeon lol ")
            .title_alignment(Alignment::Center)
            .border_set(symbols::border::THICK);

        (&block).render(area, buf);
        let inner = block.inner(area);
        self.render_inner(inner, buf);
    }
}

impl<'a> FloorWidget<'a> {
    fn render_inner(self, area: Rect, buf: &mut Buffer)
    where
        Self: Sized,
    {
        let mut camera = Camera {
            look_at: self.look_at,
            render_target: area,
            buffer: buf,
        };

        self.render_background(&mut camera);
        self.render_floor(&mut camera);
        self.render_walls(&mut camera);
        self.render_creatures(&mut camera);

        self.overlay_grid(&mut camera);
        self.render_vi_keys(&mut camera);
        self.render_timekeeping(&mut camera);
    }

    fn render_background(&self, camera: &mut Camera) {
        for (cell, _position) in camera.iter_mut_cell_and_world() {
            cell.set_bg(Color::Indexed(232));
        }
    }

    fn render_floor(&self, camera: &mut Camera) {
        for (cell, position) in camera.iter_mut_cell_and_world() {
            let oval = position.0 * position.0 / 4 + position.1 * position.1;
            if oval < 20 {
                cell.set_char('.');
                cell.set_bg(Color::Indexed(0));
            }
        }
    }

    fn render_walls(&self, camera: &mut Camera) {
        for (cell, position) in camera.iter_mut_cell_and_world() {
            let oval = position.0 * position.0 / 4 + position.1 * position.1;
            if (20..36).contains(&oval) {
                cell.set_char('#');
                cell.set_bg(Color::Indexed(240));
            }
        }
    }

    fn render_creatures(&self, camera: &mut Camera) {
        let turntaker_id = self.floor.try_into_turntaker().expect("for now").get_id();

        for (id, creature) in self.floor.get_creature_list().iter() {
            let worldspace = creature.get_flat_position();

            if let Some(cell) = camera.cell_mut(worldspace) {
                cell.set_char(creature.get_char());
                cell.set_fg(Color::Indexed(creature.get_fg_color()));

                if id == turntaker_id {
                    cell.set_style(cell.style().italic());
                }
            }
        }
    }

    fn overlay_grid(&self, camera: &mut Camera) {
        for (cell, position) in camera.iter_mut_cell_and_world() {
            if (position.1.rem_euclid(8) == 3 || position.0.rem_euclid(8) == 3)
                && let Color::Indexed(i) = cell.bg
            {
                let slight_alteration = if i == 0 {
                    232
                } else if i < 16 {
                    i
                } else {
                    i + 1
                };
                cell.set_bg(Color::Indexed(slight_alteration));
            }
        }
    }

    fn render_vi_keys(&self, camera: &mut Camera) {
        let x = camera.render_target.x + 2;
        let y = camera.render_target.y + 2;

        let mut set_char = |x, y, ch| {
            camera.buffer.cell_mut((x, y)).map(|x| x.set_char(ch));
        };

        set_char(x, y, '.');

        set_char(x - 1, y, '-');
        set_char(x, y + 1, '|');
        set_char(x, y - 1, '|');
        set_char(x + 1, y, '-');

        set_char(x - 1, y - 1, '\\');
        set_char(x + 1, y - 1, '/');
        set_char(x - 1, y + 1, '/');
        set_char(x + 1, y + 1, '\\');

        set_char(x - 2, y, 'h');
        set_char(x, y + 2, 'j');
        set_char(x, y - 2, 'k');
        set_char(x + 2, y, 'l');

        set_char(x - 2, y - 2, 'y');
        set_char(x + 2, y - 2, 'u');
        set_char(x - 2, y + 2, 'b');
        set_char(x + 2, y + 2, 'n');
    }

    fn render_timekeeping(&self, camera: &mut Camera) {
        let turntaker = self.floor.try_into_turntaker().unwrap();

        let formatted = format!("Time: {}", turntaker.get_now());

        let x = camera.render_target.x;
        let y = camera.render_target.y + camera.render_target.height - 1;

        camera.buffer.set_string(x, y, formatted, Style::default());

        let my_turn_next_round = turntaker.get_now().skip_rounds(1);

        for (i, (id, turn, creature)) in
            self.floor.get_creature_list().iter_turn_order().enumerate()
        {
            if let Some(cell) = camera.buffer.cell_mut((x + i as u16, y - 1)) {
                cell.set_char(creature.get_char());
                cell.set_fg(Color::Indexed(creature.get_fg_color()));

                if turn < my_turn_next_round {
                    cell.set_style(cell.style().italic());
                }
            }

            if let Some(cell) = camera.buffer.cell_mut((x + i as u16, y - 2)) {
                let round = turn.coming_round_for(id) % 10;
                let char = ['0', '1', '2', '3', '4', '5', '6', '7', '8', '9'].index(round as usize);
                cell.set_char(*char);
            }
        }
    }
}
