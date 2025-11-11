use HexDigit::*;
use lizstate_sequence::enum_deque::EnumDeque;
use lizstate_sequence::fieldless_enum::IsReprU8;

#[derive(Debug, Clone, Copy, PartialEq, Eq)]
#[repr(u8)]
enum HexDigit {
    Zero,
    One,
    Two,
    Three,
    Four,
    Five,
    Six,
    Seven,
    Eight,
    Nine,
    A,
    B,
    C,
    D,
    E,
    F,
}

impl IsReprU8 for HexDigit {
    const ENUM: &[Self] = &[
        Zero, One, Two, Three, Four, Five, Six, Seven, Eight, Nine, A, B, C, D, E, F,
    ];
}

impl From<u8> for HexDigit {
    fn from(value: u8) -> Self {
        Self::new_from_value(value)
    }
}

impl From<HexDigit> for u8 {
    fn from(value: HexDigit) -> Self {
        value as u8
    }
}

#[test]
fn hex_representation() {
    let mut deque = EnumDeque::<HexDigit, 16, 15>::new_empty();
    deque.push_low(One).unwrap();
    deque.push_low(Three).unwrap();
    deque.push_low(Three).unwrap();
    deque.push_low(Seven).unwrap();
    assert!(deque.into_iter_high_to_low().eq([One, Three, Three, Seven]));

    let mut deque = EnumDeque::<HexDigit, 16, 15>::new_empty();
    deque.push_high(Eight).unwrap();
    deque.push_high(Seven).unwrap();
    deque.push_high(Six).unwrap();
    deque.push_high(Five).unwrap();
    deque.push_high(Three).unwrap();
    deque.push_high(Zero).unwrap();
    deque.push_high(Nine).unwrap();
    assert!(
        deque
            .into_iter_low_to_high()
            .eq([Eight, Seven, Six, Five, Three, Zero, Nine])
    );
}

#[test]
fn fits_exactly_fifteen() {
    let mut deque = EnumDeque::<HexDigit, 16, 15>::new_empty();
    for _ in 0..15 {
        deque.push_low(F).unwrap();
    }
}
