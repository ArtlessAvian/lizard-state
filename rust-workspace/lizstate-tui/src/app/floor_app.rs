use lizstate_crawler::entity::Entity;
use lizstate_crawler::floor::Floor;
use ratatui::crossterm::event::KeyModifiers;
use ratatui::crossterm::event::{self};
use ratatui::prelude::*;
use ratatui::widgets::*;

use crate::FORCE_CLOSE;
use crate::app::AppState;
use crate::helper;

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
        self.render_inner(&inner, buf);
    }
}

impl<'a> FloorWidget<'a> {
    fn render_inner(self, area: &Rect, buf: &mut Buffer)
    where
        Self: Sized,
    {
        self.render_floor(area, buf);
        self.render_grid(area, buf);
        self.render_creatures(area, buf);
    }

    fn render_grid(&self, area: &Rect, buf: &mut Buffer) {
        for (cell, position) in helper::zip_cells_and_worldspace(area, buf) {
            if position.0 % 8 == 0 || position.1 % 8 == 0 {
                cell.set_bg(Color::DarkGray);
            }
        }
    }

    fn render_floor(&self, _area: &Rect, _buf: &mut Buffer) {}

    fn render_creatures(&self, area: &Rect, buf: &mut Buffer) {
        for creature in self.0.get_creatures() {
            let world_pos = creature.get_flat_position();

            if let Some(cell) = helper::world_to_cell(area, buf, world_pos) {
                cell.set_char(creature.get_char());
                cell.set_style(Style::default());
            }
        }
    }
}
