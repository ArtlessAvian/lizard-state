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
    actionable_round: u32,
    color: u32,
}

impl<Pos: GridLike> Creature<Pos> {
    pub fn new(pos: Pos, first_round: u32, color: u32) -> Self {
        Self {
            pos,
            actionable_round: first_round,
            color,
        }
    }

    pub fn step(&mut self, dir: KingStep) {
        self.pos = self.pos.step_king(dir);
    }

    pub fn get_position(&self) -> Pos {
        self.pos
    }

    #[expect(clippy::unnecessary_wraps, reason = "todo")]
    pub fn get_round(&self) -> Option<u32> {
        Some(self.actionable_round)
    }

    /// This will not be the actual thingy.
    pub fn set_round(&mut self, round: u32) {
        self.actionable_round = round;
    }
}

impl<Pos: GridLike> Entity for &Creature<Pos> {
    fn get_char(&self) -> char {
        '@'
    }

    fn get_fg_color(&self) -> u8 {
        const REMAP: u8 = 43; // ceil(256/6);

        let yeah = self.color.to_be_bytes();
        get_six_bit_color(yeah[0] / REMAP, yeah[1] / REMAP, yeah[2] / REMAP)
    }

    fn get_flat_position(&self) -> (i32, i32) {
        self.pos.flatten()
    }
}
