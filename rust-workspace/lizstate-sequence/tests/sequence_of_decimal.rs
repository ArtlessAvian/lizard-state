use DecimalDigits::*;
use lizstate_sequence::enum_deque::EnumDeque;
use lizstate_sequence::fieldless_enum::IsReprU8;

#[derive(Debug, Clone, Copy, PartialEq, Eq)]
#[repr(u8)]
enum DecimalDigits {
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
}

impl IsReprU8 for DecimalDigits {
    const ENUM: &'static [Self] = &[Zero, One, Two, Three, Four, Five, Six, Seven, Eight, Nine];
}

impl From<u8> for DecimalDigits {
    fn from(value: u8) -> Self {
        Self::new_from_value(value)
    }
}

impl From<DecimalDigits> for u8 {
    fn from(value: DecimalDigits) -> Self {
        value as u8
    }
}

#[test]
fn decimal_representation() {
    let mut deque = EnumDeque::<DecimalDigits, 10, 8>::new_empty();
    deque.push_low(One).unwrap();
    deque.push_low(Three).unwrap();
    deque.push_low(Three).unwrap();
    deque.push_low(Seven).unwrap();
    assert!(deque.into_iter_high_to_low().eq([One, Three, Three, Seven]));

    let mut deque = EnumDeque::<DecimalDigits, 10, 8>::new_empty();
    deque.push_low(One).unwrap();
    deque.push_low(Three).unwrap();
    deque.push_low(Three).unwrap();
    deque.push_low(Seven).unwrap();
    assert!(deque.into_iter_high_to_low().eq([One, Three, Three, Seven]));
}
