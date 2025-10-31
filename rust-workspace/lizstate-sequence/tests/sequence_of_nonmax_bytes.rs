use lizstate_sequence::digit::Digit;
use lizstate_sequence::digit::IsSmallEnum;
use lizstate_sequence::element_deque::PackedDeque;

#[derive(Debug, PartialEq, Eq)]
struct NonMaxByte(u8);

impl IsSmallEnum for NonMaxByte {
    type Digit = Digit<255>;

    fn to_digit(&self) -> Self::Digit {
        assert!(self.0 != 0xFF);
        Digit::from_modulo_u8(self.0)
    }

    fn from_digit(digit: Self::Digit) -> Self {
        NonMaxByte(digit.get())
    }
}

#[test]
fn byte_representation() {
    let mut deque = PackedDeque::<NonMaxByte, 255, 8>::new_empty();

    deque.push_low(NonMaxByte(0x01)).unwrap();
    deque.push_low(NonMaxByte(0x03)).unwrap();
    deque.push_low(NonMaxByte(0x03)).unwrap();
    deque.push_low(NonMaxByte(0x07)).unwrap();

    assert_eq!(deque.peek_low().unwrap(), NonMaxByte(0x07));
    deque.pop_low().unwrap();
    assert_eq!(deque.peek_low().unwrap(), NonMaxByte(0x03));
    deque.pop_low().unwrap();
    assert_eq!(deque.peek_low().unwrap(), NonMaxByte(0x03));
    deque.pop_low().unwrap();
    assert_eq!(deque.peek_low().unwrap(), NonMaxByte(0x01));
    deque.pop_low().unwrap();
}

#[test]
fn fits_exactly_eight() {
    let mut deque = PackedDeque::<NonMaxByte, 255, 8>::new_empty();

    for _ in 0..8 {
        deque.push_low(NonMaxByte(0xFE)).unwrap();
    }

    deque.push_low(NonMaxByte(0xFE)).expect_err("overflow!");

    for _ in 0..8 {
        assert_eq!(deque.peek_low().unwrap(), NonMaxByte(0xFE));
        deque.pop_low().unwrap();
    }
}
