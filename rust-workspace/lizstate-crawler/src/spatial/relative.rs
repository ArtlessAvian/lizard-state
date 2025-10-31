#[derive(Clone, Copy)]
pub enum Cardinal {
    North,
    South,
    East,
    West,
}

#[derive(Clone, Copy)]
pub enum Diagonal {
    NorthEast,
    NorthWest,
    SouthEast,
    SouthWest,
}
