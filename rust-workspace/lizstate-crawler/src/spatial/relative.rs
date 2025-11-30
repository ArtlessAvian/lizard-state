/// Generating set of `Gridlike` implementors.
///
/// Also the generating set of `Pathlike`s.
/// `Gridlikes` behave like absolute positions.
/// `Pathlikes` behave like relative positions.
#[derive(Clone, Copy)]
pub enum Cardinal {
    North,
    South,
    East,
    West,
}

/// Like a King in chess.
///
/// IRONICALLY. This behaves more like a horsey which moves in an L, except without a long end.
#[derive(Debug, Clone, Copy)]
pub enum KingStep {
    North,
    South,
    East,
    West,
    NorthEast,
    NorthWest,
    SouthEast,
    SouthWest,
}

impl KingStep {
    #[must_use]
    pub fn decompose(self) -> (Option<Cardinal>, Option<Cardinal>) {
        match self {
            KingStep::North => (Some(Cardinal::North), None),
            KingStep::South => (Some(Cardinal::South), None),
            KingStep::East => (None, Some(Cardinal::East)),
            KingStep::West => (None, Some(Cardinal::West)),
            KingStep::NorthEast => (Some(Cardinal::North), Some(Cardinal::East)),
            KingStep::NorthWest => (Some(Cardinal::North), Some(Cardinal::West)),
            KingStep::SouthEast => (Some(Cardinal::South), Some(Cardinal::East)),
            KingStep::SouthWest => (Some(Cardinal::South), Some(Cardinal::West)),
        }
    }
}
