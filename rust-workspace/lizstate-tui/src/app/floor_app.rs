use lizstate_crawler::entity::Entity;
use lizstate_crawler::floor::Floor;
use ratatui::crossterm::event::KeyModifiers;
use ratatui::crossterm::event::{self};
use ratatui::prelude::*;
use ratatui::widgets::*;

use crate::FORCE_CLOSE;
use crate::app::AppState;

pub struct FloorWidget<'a>(&'a Floor);

pub struct FloorState {
    pub exit: bool,
    floor: Floor,
}

impl FloorState {
    pub fn new_test() -> Self {
        Self {
            exit: false,
            floor: Floor::new_test(),
        }
    }
}

impl AppState for FloorState {
    fn render(&self, frame: &mut Frame, area: Rect) {
        frame.render_widget(FloorWidget(&self.floor), area);
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
        self.render_floor(inner, buf);
    }
}

impl<'a> FloorWidget<'a> {
    fn render_floor(self, area: Rect, buf: &mut Buffer)
    where
        Self: Sized,
    {
        for creature in self.0.get_creatures() {
            let world_pos = creature.get_flat_position();

            FloorWidget::render_char(area, buf, world_pos, creature.get_char(), Style::default());
        }
    }

    fn render_char<S: Into<Style>>(
        area: Rect,
        buf: &mut Buffer,
        world_pos: (i32, i32),
        char: char,
        style: S,
    ) {
        let center: Position = (
            (area.left() + area.right()) / 2,
            (area.top() + area.bottom()) / 2,
        )
            .into();

        let screen: Position = (
            (world_pos.0 + center.x as i32) as u16,
            (world_pos.1 + center.y as i32) as u16,
        )
            .into();

        if area.contains(screen) {
            let cell = buf.cell_mut(screen).expect("its inside the area");
            cell.set_style(style);
            cell.set_char(char);
        }
    }
}
