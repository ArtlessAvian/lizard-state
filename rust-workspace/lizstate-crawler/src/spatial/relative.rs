#[derive(Clone, Copy)]
#[repr(u8)]
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

#[derive(Clone, Copy)]
pub struct Vector2i(pub i32, pub i32);

pub trait Pathlike: Sized {
    fn new_empty() -> Self;
    fn len(&self) -> usize;
    fn is_empty(&self) -> bool {
        self.len() == 0
    }

    fn flatten(&self) -> Vector2i;
    fn peek(&self) -> Option<Cardinal>;

    fn append_cardinal(self, dir: Cardinal) -> Option<Self>;
    fn pop(self) -> Option<Self>;
}

// pub struct Path(PackedDeque<Cardinal, 4, 31>);

// impl Pathlike for Path {}
