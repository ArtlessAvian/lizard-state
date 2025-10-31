use lizstate_sequence::digit::Digit;
use lizstate_sequence::digit::IsSmallEnum;
use lizstate_sequence::element_deque::PackedDeque;

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

impl IsSmallEnum for DecimalDigits {
    type Digit = Digit<10>;

    fn to_digit(&self) -> Self::Digit {
        Digit::from_modulo_u8(*self as u8)
    }

    fn from_digit(digit: Self::Digit) -> Self {
        match digit.get() {
            0 => Self::Zero,
            1 => Self::One,
            2 => Self::Two,
            3 => Self::Three,
            4 => Self::Four,
            5 => Self::Five,
            6 => Self::Six,
            7 => Self::Seven,
            8 => Self::Eight,
            9 => Self::Nine,
            (10..) => {
                unreachable!()
            }
        }
    }
}

#[test]
fn decimal_representation() {
    let mut deque = PackedDeque::<DecimalDigits, 10, 19>::new_empty();

    deque.push_low(DecimalDigits::One).unwrap();
    deque.push_low(DecimalDigits::Three).unwrap();
    deque.push_low(DecimalDigits::Three).unwrap();
    deque.push_low(DecimalDigits::Seven).unwrap();

    assert_eq!(deque.get(), 1337 + 1111);
}
