use crate::math::generating::GeneratingFunction;

/// A user defined generating function, made from a
/// a fininte sequence and ending in infinite zeroes.
///
/// This is not supposed to be a queue.
/// (We will mangle it to be one later.)
/// We cannot tell if a sequence is empty since it "looks the same"
/// as a sequence with an arbitrary number of zeroes.
#[derive(Debug, Clone, Copy, Default, PartialEq, Eq, PartialOrd, Ord, Hash)]
pub struct Sequence<const BASE: u8>(u64);

impl<const BASE: u8> GeneratingFunction for Sequence<BASE> {
    type Field = u64;
    const X: Self::Field = BASE as u64;

    fn get_value(&self) -> Self::Field {
        self.0
    }

    fn from_value(val: Self::Field) -> Self {
        Self(val)
    }
}

#[cfg(test)]
mod tests {
    use crate::math::generating::GeneratingFunction;
    use crate::math::generating::terminating::Sequence;
    use crate::math::power_series::PowerSeriesAlgebra;

    #[test]
    fn sequence_from_iter() {
        let array = [1, 3, 3, 7];
        let sequence = Sequence::<16>::from_iter(array);
        assert_eq!(sequence.0, 0x7331);

        let mut coeffs = sequence.into_iter_coeffs();
        let mut roundtrip = [u8::MAX; 10];
        roundtrip.fill_with(|| coeffs.next().unwrap());
        assert_eq!(roundtrip, [1, 3, 3, 7, 0, 0, 0, 0, 0, 0]);

        let infinite_zeroes = coeffs;
        assert_eq!(coeffs.next().unwrap(), 0);
        assert_eq!(coeffs, infinite_zeroes);
    }

    #[test]
    fn capacity_is_weird() {
        Sequence::<10>::from_iter([7; 16]);
        Sequence::<10>::from_iter([1; 17]);
    }

    #[test]
    #[should_panic = "attempt to multiply with overflow"]
    fn overflow() {
        Sequence::<10>::from_iter([7; 100]);
    }
}
