use ratatui::prelude::*;

use crate::app::floor_app::FloorState;

mod floor_app;

#[enum_delegate::register]
pub trait AppState {
    fn render(&self, frame: &mut Frame, area: Rect);
    fn handle_events(&mut self) -> std::io::Result<()>;
}

#[enum_delegate::implement(AppState)]
pub enum App {
    FloorState(FloorState),
}

impl App {
    pub fn new() -> Self {
        App::FloorState(FloorState::new_test())
    }
}
