use ratatui::prelude::*;

use crate::app::floor_app::FloorState;

mod floor_app;

pub trait AppState {
    fn render(&self, frame: &mut Frame, area: Rect);
    fn handle_events(&mut self) -> std::io::Result<()>;
}

pub enum App {
    FloorState(FloorState),
}

impl App {
    pub fn new() -> Self {
        App::FloorState(FloorState::new_test())
    }
}

impl AppState for App {
    fn render(&self, frame: &mut Frame, area: Rect) {
        match self {
            App::FloorState(floor_state) => floor_state.render(frame, area),
        }
    }

    fn handle_events(&mut self) -> std::io::Result<()> {
        match self {
            App::FloorState(floor_state) => floor_state.handle_events(),
        }
    }
}
