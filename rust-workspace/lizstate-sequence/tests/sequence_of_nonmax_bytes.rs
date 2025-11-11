use lizstate_sequence::enum_deque::EnumDeque;
use lizstate_sequence::fieldless_enum::IsReprU8;

#[derive(Debug, PartialEq, Eq, Clone, Copy)]
struct NonMaxByte(u8);

impl IsReprU8 for NonMaxByte {
    const ENUM: &[Self] = &{
        let mut out = [NonMaxByte(0); 254];
        let mut i = 0;
        while i < out.len() {
            out[i] = NonMaxByte(i as u8);
            i += 1;
        }
        out
    };
}

impl From<NonMaxByte> for u8 {
    fn from(value: NonMaxByte) -> Self {
        value.0
    }
}

#[test]
fn byte_representation() {
    let mut deque = EnumDeque::<NonMaxByte, 255, 8>::new_empty();

    deque.push_low(NonMaxByte(0x01)).unwrap();
    deque.push_low(NonMaxByte(0x03)).unwrap();
    deque.push_low(NonMaxByte(0x03)).unwrap();
    deque.push_low(NonMaxByte(0x07)).unwrap();

    assert_eq!(deque.pop_low().unwrap(), NonMaxByte(0x07));
    assert_eq!(deque.pop_low().unwrap(), NonMaxByte(0x03));
    assert_eq!(deque.pop_low().unwrap(), NonMaxByte(0x03));
    assert_eq!(deque.pop_low().unwrap(), NonMaxByte(0x01));
    deque.pop_low().expect_err("empty");
}
