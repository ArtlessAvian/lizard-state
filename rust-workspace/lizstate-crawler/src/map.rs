pub enum Cardinal {
    North,
    South,
    East,
    West,
}

pub trait GridLike: Sized + Copy {
    fn flatten(self) -> (i32, i32);
    /// Flattens into (0, 0).
    fn origin() -> Self;
    fn step(self, dir: Cardinal) -> Option<Self>;
}

// The Gridlike that flattens into itself.
#[derive(Clone, Copy)]
pub struct GridPos(pub i32, pub i32);

impl GridLike for GridPos {
    fn flatten(self) -> (i32, i32) {
        (self.0, self.1)
    }

    fn origin() -> Self {
        GridPos(0, 0)
    }

    fn step(self, dir: Cardinal) -> Option<Self> {
        Some(match dir {
            Cardinal::North => GridPos(self.0, self.1 - 1),
            Cardinal::South => GridPos(self.0, self.1 + 1),
            Cardinal::East => GridPos(self.0 + 1, self.1),
            Cardinal::West => GridPos(self.0 - 1, self.1),
        })
    }
}

// /// I don't have a nice name for this yet.
// pub struct QuiltPosition([Cardinal; 8], i8, i8);
