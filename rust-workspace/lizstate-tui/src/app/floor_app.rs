use lizstate_crawler::entity::Entity;
use lizstate_crawler::floor::Floor;
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

        match key_event.code {
            event::KeyCode::Char('h') => {}
            event::KeyCode::Char('j') => {}
            event::KeyCode::Char('k') => {}
            event::KeyCode::Char('l') => {}
            _ => {}
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
    }

    fn render_background(&self, camera: &mut Camera) {
        for (cell, _position) in camera.iter_mut_cell_and_world() {
            cell.set_bg(Color::Indexed(232));
        }
    }

    fn render_floor(&self, camera: &mut Camera) {
        for (cell, position) in camera.iter_mut_cell_and_world() {
            let lol = position.0 * position.0 / 4 + position.1 * position.1;
            if lol < 20 {
                cell.set_char('.');
                cell.set_bg(Color::Indexed(0));
            }
        }
    }

    fn render_walls(&self, camera: &mut Camera) {
        for (cell, position) in camera.iter_mut_cell_and_world() {
            let lol = position.0 * position.0 / 4 + position.1 * position.1;
            if (20..36).contains(&lol) {
                cell.set_char('#');
                cell.set_bg(Color::Indexed(240));
            }
        }
    }

    fn render_creatures(&self, camera: &mut Camera) {
        for creature in self.floor.get_creatures() {
            let worldspace = creature.get_flat_position();

            if let Some(cell) = camera.cell_mut(worldspace) {
                cell.set_char(creature.get_char());
                cell.set_fg(Color::Indexed(creature.get_fg_color()));
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
}
