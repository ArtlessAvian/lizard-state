use lizstate_sequence::digit::Digit;
use lizstate_sequence::digit::IsSmallEnum;
use lizstate_sequence::element_deque::PackedDeque;

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

impl IsSmallEnum for HexDigit {
    type Digit = Digit<16>;

    fn to_digit(&self) -> Self::Digit {
        Digit::from_modulo_u8(*self as u8)
    }

    fn from_digit(digit: Self::Digit) -> Self {
        match digit.get() {
            0x0 => Self::Zero,
            0x1 => Self::One,
            0x2 => Self::Two,
            0x3 => Self::Three,
            0x4 => Self::Four,
            0x5 => Self::Five,
            0x6 => Self::Six,
            0x7 => Self::Seven,
            0x8 => Self::Eight,
            0x9 => Self::Nine,
            0xA => Self::A,
            0xB => Self::B,
            0xC => Self::C,
            0xD => Self::D,
            0xE => Self::E,
            0xF => Self::F,
            (16..) => {
                unreachable!()
            }
        }
    }
}

#[test]
fn hex_representation() {
    let mut deque = PackedDeque::<HexDigit, 16, 15>::new_empty();

    deque.push_low(HexDigit::One).unwrap();
    deque.push_low(HexDigit::Three).unwrap();
    deque.push_low(HexDigit::Three).unwrap();
    deque.push_low(HexDigit::Seven).unwrap();

    assert_eq!(deque.get(), 0x1337 + 0x1111);
}

#[test]
fn fits_exactly_fifteen() {
    let mut deque = PackedDeque::<HexDigit, 16, 15>::new_empty();

    for _ in 0..15 {
        deque.push_low(HexDigit::F).unwrap();
    }

    assert_eq!(deque.get(), 0x0FFF_FFFF_FFFF_FFFF + 0x0111_1111_1111_1111);
}
