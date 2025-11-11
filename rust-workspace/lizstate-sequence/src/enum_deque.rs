use core::marker::PhantomData;

use crate::deque::Deque;
use crate::deque::PopError;
use crate::deque::PushError;
use crate::fieldless_enum::IsReprU8;
use crate::fieldless_enum::enum_from_value;

/// EnumDeque, impossible to push OOB, but not inherently useful.
pub type NaiveEnumDeque<T> = EnumDeque<T, 256, 7>;

#[derive(Debug, Clone, Copy)]
pub struct EnumDeque<T: IsReprU8, const BOUND_EXCLUSIVE: u16, const CAPACITY: u8 = 8>(
    Deque<BOUND_EXCLUSIVE, CAPACITY>,
    PhantomData<T>,
);

impl<T: IsReprU8, const BOUND: u16, const CAP: u8> EnumDeque<T, BOUND, CAP> {
    pub const fn new_empty() -> Self {
        Self(Deque::new_empty(), PhantomData)
    }

    pub fn is_empty(&self) -> bool {
        self.0.is_empty()
    }

    pub const fn len(&self) -> usize {
        self.0.len()
    }

    pub const fn is_full(&self) -> bool {
        self.0.is_full()
    }

    pub fn push_low(&mut self, el: T) -> Result<(), PushError> {
        self.0.push_low(Into::<u8>::into(el))
    }

    pub fn pop_low(&mut self) -> Result<T, PopError> {
        self.0.pop_low().map(enum_from_value)
    }

    pub fn push_high(&mut self, el: T) -> Result<(), PushError> {
        self.0.push_high(Into::<u8>::into(el))
    }

    pub fn pop_high(&mut self) -> Result<T, PopError> {
        self.0.pop_high().map(enum_from_value)
    }

    pub fn into_iter_low_to_high(self) -> impl Iterator<Item = T> {
        self.0.into_iter_low_to_high().map(enum_from_value)
    }

    pub fn into_iter_high_to_low(self) -> impl Iterator<Item = T> {
        self.0.into_iter_high_to_low().map(enum_from_value)
    }
}
