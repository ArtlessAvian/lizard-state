use crate::entity::Entity;
use crate::entity::get_six_bit_color;
use crate::spatial::grid::GridLike;
use crate::spatial::grid::GridPosition;
use crate::spatial::grid::KingStep;

#[derive(Debug, Clone)]
#[non_exhaustive]
#[must_use]
pub struct Creature<Pos: GridLike = GridPosition> {
    pos: Pos,
}

impl<Pos: GridLike> Creature<Pos> {
    pub fn new(pos: Pos) -> Self {
        Self { pos }
    }

    pub fn step(&mut self, dir: KingStep) {
        self.pos = self.pos.step_king(dir);
    }

    pub fn get_position(&self) -> Pos {
        self.pos
    }
}

impl<Pos: GridLike> Entity for &Creature<Pos> {
    fn get_char(&self) -> char {
        '@'
    }

    fn get_fg_color(&self) -> u8 {
        get_six_bit_color(1, 0, 0)
    }

    fn get_flat_position(&self) -> (i32, i32) {
        self.pos.flatten()
    }
}
