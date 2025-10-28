pub trait Entity {
    /// This is not useful for graphical clients, but can be used for fun.
    fn get_char(&self) -> char;

    /// This is naive and ignores layers and vision.
    fn get_flat_position(&self) -> (i32, i32);
}
