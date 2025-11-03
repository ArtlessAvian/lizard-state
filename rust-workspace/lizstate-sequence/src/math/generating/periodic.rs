use crate::math::generating::GeneratingFunction;
use crate::math::ring::fixed::FixedFraction;
use crate::math::ring::fraction::Fraction;

#[derive(Debug, Clone, Copy, Default, PartialEq, PartialOrd)]
struct SequenceFraction<const BASE: u8>(Fraction);

impl<const BASE: u8> GeneratingFunction for SequenceFraction<BASE> {
    type Field = Fraction;
    const X: Self::Field = Fraction(1, BASE as u64);

    fn from_value(val: Self::Field) -> Self {
        Self(val)
    }

    fn get_value(&self) -> Self::Field {
        self.0
    }
}

/// You have to carefully choose your denominator here. It's kind of awkward.
#[derive(Debug, Clone, Copy, Default, PartialEq, PartialOrd)]
struct SequenceFixedFraction<const BASE: u8, const DENOM: u64>(FixedFraction<DENOM>);

impl<const BASE: u8, const DENOM: u64> GeneratingFunction for SequenceFixedFraction<BASE, DENOM> {
    type Field = FixedFraction<DENOM>;
    const X: Self::Field = {
        assert!(DENOM.is_multiple_of(BASE as u64));
        FixedFraction::<DENOM>(DENOM / BASE as u64)
    };

    fn from_value(val: Self::Field) -> Self {
        Self(val)
    }

    fn get_value(&self) -> Self::Field {
        self.0
    }
}

#[cfg(test)]
mod tests {
    use std::dbg;

    use crate::math::generating::GeneratingFunction;
    use crate::math::generating::periodic::SequenceFixedFraction;
    use crate::math::generating::periodic::SequenceFraction;
    use crate::math::power_series::PowerSeriesAlgebra;
    use crate::math::ring::fixed::FixedFraction;
    use crate::math::ring::fraction::Fraction;

    #[test]
    fn fraction_infinite() {
        let sequence = SequenceFraction::<10>::from_value(Fraction(70, 9));

        let mut roundtrip = sequence.into_iter_coeffs();
        let mut junk = [0; 100];
        junk.fill_with(|| {
            dbg!(roundtrip);
            roundtrip.next().unwrap()
        });

        let all_sevens = junk.iter().all(|x| *x == 7);
        assert!(all_sevens, "{junk:?}");

        assert_eq!(roundtrip, sequence.into_iter_coeffs());
    }

    #[test]
    fn fixed_fraction_infinite() {
        let sequence = SequenceFixedFraction::<10, 90>::from_value(FixedFraction::<90>(700));

        let mut roundtrip = sequence.into_iter_coeffs();
        let mut junk = [0; 100];
        junk.fill_with(|| {
            dbg!(roundtrip);
            roundtrip.next().unwrap()
        });

        let all_sevens = junk.iter().all(|x| *x == 7);
        assert!(all_sevens, "{junk:?}");

        assert_eq!(roundtrip, sequence.into_iter_coeffs());
    }
}
