pub trait Entity {
    /// This is not useful for graphical clients, but can be used for fun.
    fn get_char(&self) -> char;
    fn get_fg_color(&self) -> u8;

    /// This is naive and ignores layers and vision.
    fn get_flat_position(&self) -> (i32, i32);
}

#[must_use]
pub fn get_six_bit_color(r: u8, g: u8, b: u8) -> u8 {
    16 + 36 * r + 6 * g + b
}
