use Cardinal::*;
use lizstate_sequence::enum_deque::EnumDeque;
use lizstate_sequence::fieldless_enum::IsReprU8;

#[derive(Debug, Clone, Copy, PartialEq, Eq)]
#[repr(u8)]
enum Cardinal {
    N,
    E,
    S,
    W,
}

impl IsReprU8 for Cardinal {
    const ENUM: &'static [Self] = &[N, E, S, W];
}

impl From<u8> for Cardinal {
    fn from(value: u8) -> Self {
        Self::new_from_value(value)
    }
}

impl From<Cardinal> for u8 {
    fn from(value: Cardinal) -> Self {
        value as u8
    }
}

#[test]
fn high_capacity() {
    EnumDeque::<Cardinal, 4, 31>::new_empty();
}
