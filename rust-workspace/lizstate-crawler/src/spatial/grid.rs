use std::hash::Hash;

use crate::spatial::relative::Cardinal;
use crate::spatial::relative::KingStep;

/// Locations reached by taking steps from the origin.
///
/// Implementors are allowed to wrap and panic.
pub trait GridLike: Sized + Copy + Eq + Hash {
    /// The difference of East and West steps, and South and North steps.
    fn flatten(self) -> (i32, i32);
    /// Flattens into (0, 0).
    fn origin() -> Self;

    /// Allowed to wrap.
    /// # Panics
    /// Implementor is allowed to panic.
    #[must_use]
    fn step(self, dir: Cardinal) -> Self;

    #[must_use]
    fn step_option(self, dir: Option<Cardinal>) -> Self {
        dir.map_or(self, |dir| self.step(dir))
    }

    #[must_use]
    fn step_king(self, dir: KingStep) -> Self {
        let (horizontal, vertical) = dir.decompose();
        let first = self.step_option(horizontal);
        first.step_option(vertical)
    }

    #[must_use]
    fn step_option_king(self, dir: Option<KingStep>) -> Self {
        dir.map_or(self, |dir| self.step_king(dir))
    }
}

// The Gridlike that flattens into itself.
#[derive(Debug, Clone, Copy, PartialEq, Eq, Hash)]
pub struct GridPosition(pub i32, pub i32);

impl GridLike for GridPosition {
    fn flatten(self) -> (i32, i32) {
        (self.0, self.1)
    }

    fn origin() -> Self {
        GridPosition(0, 0)
    }

    fn step(self, dir: Cardinal) -> Self {
        match dir {
            Cardinal::North => GridPosition(self.0, self.1 - 1),
            Cardinal::South => GridPosition(self.0, self.1 + 1),
            Cardinal::East => GridPosition(self.0 + 1, self.1),
            Cardinal::West => GridPosition(self.0 - 1, self.1),
        }
    }
}
