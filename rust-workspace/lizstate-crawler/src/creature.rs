use crate::entity::Entity;
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

    fn get_flat_position(&self) -> (i32, i32) {
        self.pos.flatten()
    }
}
