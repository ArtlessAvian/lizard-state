use crate::entity::Entity;
use crate::entity::get_six_bit_color;
use crate::map::GridLike;
use crate::map::GridPos;

#[non_exhaustive]
pub struct Creature<Pos: GridLike = GridPos> {
    pos: Pos,
}

impl<Pos: GridLike> Creature<Pos> {
    pub fn new(pos: Pos) -> Self {
        Self { pos }
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
