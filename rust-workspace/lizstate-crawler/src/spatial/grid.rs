use crate::spatial::relative::Cardinal;
use crate::spatial::relative::Diagonal;

pub trait GridLike: Sized + Copy {
    fn flatten(self) -> (i32, i32);
    /// Flattens into (0, 0).
    fn origin() -> Self;
    fn step(self, dir: Cardinal) -> Option<Self>;

    fn step_diagonal(self, dir: Diagonal) -> Option<Self> {
        match dir {
            Diagonal::NorthEast => self
                .step(Cardinal::North)
                .and_then(|pos| pos.step(Cardinal::East)),
            Diagonal::NorthWest => self
                .step(Cardinal::North)
                .and_then(|pos| pos.step(Cardinal::West)),
            Diagonal::SouthEast => self
                .step(Cardinal::South)
                .and_then(|pos| pos.step(Cardinal::East)),
            Diagonal::SouthWest => self
                .step(Cardinal::South)
                .and_then(|pos| pos.step(Cardinal::West)),
        }
    }
}

// The Gridlike that flattens into itself.
#[derive(Debug, Clone, Copy, PartialEq, Eq)]
pub struct GridPosition(pub i32, pub i32);

impl GridLike for GridPosition {
    fn flatten(self) -> (i32, i32) {
        (self.0, self.1)
    }

    fn origin() -> Self {
        GridPosition(0, 0)
    }

    fn step(self, dir: Cardinal) -> Option<Self> {
        Some(match dir {
            Cardinal::North => GridPosition(self.0, self.1 - 1),
            Cardinal::South => GridPosition(self.0, self.1 + 1),
            Cardinal::East => GridPosition(self.0 + 1, self.1),
            Cardinal::West => GridPosition(self.0 - 1, self.1),
        })
    }
}
