/// Trait for fieldless enums.
pub trait IsReprU8
where
    Self: Copy + Eq + Into<u8> + 'static,
{
    const ENUM: &[Self];

    fn new_from_value(value: u8) -> Self {
        enum_from_value(value)
    }
}

/// This thing will never panic, which is nice i suppose.
pub(crate) const fn enum_from_value<T: IsReprU8>(value: u8) -> T {
    T::ENUM[value as usize % T::ENUM.len()]
}
