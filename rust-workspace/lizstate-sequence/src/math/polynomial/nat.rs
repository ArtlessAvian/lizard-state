use core::array;
use core::fmt::Display;
use core::ops::Add;
use core::ops::AddAssign;
use core::ops::Mul;
use core::ops::Sub;

use crate::math::commutative_ring::CommutativeRing;
use crate::math::commutative_ring::integers_mod::NatMod;
use crate::math::polynomial::PolynomialCoeffIterator;
use crate::math::polynomial::PolynomialRing;
use crate::math::polynomial::array::ArrayPolynomial;

#[derive(Debug, Clone, Copy, PartialEq, Eq, PartialOrd, Ord)]
pub struct NatPolynomial<const X: u16>(u64);

impl<const X: u16> Display for NatPolynomial<X> {
    fn fmt(&self, f: &mut core::fmt::Formatter<'_>) -> core::fmt::Result {
        let array_poly = ArrayPolynomial::<41>::new_from_nat_poly(self);
        array_poly.fmt(f)
    }
}

impl<const X: u16> NatPolynomial<X> {
    const LARGEST_SUPPORTED_DEGREE: usize = {
        // Goal: we can represent a polynomial with degree D with X-1 for every coefficient.
        // We control D, so we are fine if we underestimate.
        // If D is 0, then the goal is always true.

        //    (0..=D).map(|i| (X-1).pow(i)).sum() <= u64::MAX
        // => X.pow(D+1) - 1 <= u64::MAX
        // => D <= (u64::MAX + 1).log(X) - 1
        let degree = (u64::MAX as u128 + 1).ilog(X as u128) - 1;

        // Since X >= 2, then degree is at most 63,
        // which always fits in a u8, which always fits in a usize.
        degree as usize
    };

    const LARGEST_CONSTANT_POLY: Self = Self(X as u64 - 1);

    pub const fn from_raw(u64: u64) -> Self {
        Self(u64)
    }

    pub const fn get_degree(&self) -> Option<usize> {
        if let Some(at_most_64) = self.0.checked_ilog(X as u64) {
            Some(at_most_64 as usize)
        } else {
            None
        }
    }

    pub(crate) const fn leak(&self) -> u64 {
        self.0
    }

    pub fn from_array_poly<const LEN: usize>(array_poly: ArrayPolynomial<LEN>) -> Self {
        Self::new_from_coeffs(array_poly.iter_coeff()).unwrap()
    }
}

impl<const X: u16> PolynomialRing for NatPolynomial<X> {
    type Over = u8;
    const X: Self = Self(X as u64);

    fn get_constant_coeff(&self) -> u8 {
        (self.0 % (X as u64)) as u8
    }

    fn get_constant_term(&self) -> Self {
        Self::try_from(self.get_constant_coeff()).unwrap()
    }

    fn is_constant(&self) -> bool {
        self.0 < (X as u64)
    }

    fn drop_constant_and_divide_x(&mut self) {
        self.0 /= (X as u64);
    }

    fn mul_x(&mut self) {
        self.0 *= (X as u64);
    }

    fn iter_coeff(&self) -> impl Iterator<Item = Self::Over> {
        PolynomialCoeffIterator(*self)
    }

    fn get_degree(&self) -> Option<usize> {
        Self::get_degree(self)
    }
}

impl<const X: u16> TryFrom<u8> for NatPolynomial<X> {
    type Error = ();

    fn try_from(value: u8) -> Result<Self, Self::Error> {
        if (value as u64) < (X as u64) {
            Ok(Self(value as u64))
        } else {
            Err(())
        }
    }
}

impl<const X: u16> TryFrom<u64> for NatPolynomial<X> {
    type Error = ();

    fn try_from(value: u64) -> Result<Self, Self::Error> {
        if value < (X as u64) {
            Ok(Self(value))
        } else {
            Err(())
        }
    }
}

impl<const X: u16> CommutativeRing for NatPolynomial<X> {
    const ZERO: Self = Self(0);
    const ONE: Self = Self(1);
}

impl<const X: u16> Add<u8> for NatPolynomial<X> {
    type Output = Self;

    fn add(self, rhs: u8) -> Self::Output {
        Self(self.0 + rhs as u64)
    }
}

impl<const X: u16> Mul<u8> for NatPolynomial<X> {
    type Output = Self;

    fn mul(self, rhs: u8) -> Self::Output {
        Self(self.0 * rhs as u64)
    }
}

impl<const X: u16> Add<u64> for NatPolynomial<X> {
    type Output = Self;

    fn add(self, rhs: u64) -> Self::Output {
        Self(self.0 + rhs)
    }
}

impl<const X: u16> Mul<u64> for NatPolynomial<X> {
    type Output = Self;

    fn mul(self, rhs: u64) -> Self::Output {
        Self(self.0 * rhs)
    }
}

impl<const X: u16> Add for NatPolynomial<X> {
    type Output = Self;

    fn add(self, rhs: Self) -> Self::Output {
        Self(self.0 + rhs.0)
    }
}

impl<const X: u16> Mul for NatPolynomial<X> {
    type Output = Self;

    fn mul(self, rhs: Self) -> Self::Output {
        Self(self.0 * rhs.0)
    }
}

impl<const X: u16> Sub for NatPolynomial<X> {
    type Output = Self;

    fn sub(self, rhs: Self) -> Self::Output {
        Self(self.0 - rhs.0)
    }
}

#[cfg(test)]
mod tests {
    use core::ops::Add;
    use core::ops::Deref;
    use core::ops::Mul;
    use std::dbg;
    use std::println;
    use std::vec::Vec;

    use crate::math::commutative_ring::CommutativeRing;
    use crate::math::polynomial::PolynomialRing;
    use crate::math::polynomial::nat::NatPolynomial;

    #[test]
    fn consts() {
        assert_eq!(NatPolynomial::<10>::ZERO.0, 0);
        assert_eq!(NatPolynomial::<10>::ONE.0, 1);
        assert_eq!(NatPolynomial::<10>::X.0, 10);

        assert_eq!(NatPolynomial::<16>::ZERO.0, 0);
        assert_eq!(NatPolynomial::<16>::ONE.0, 1);
        assert_eq!(NatPolynomial::<16>::X.0, 16);
    }

    #[test]
    fn array_round_trip() {
        let coeffs = [8, 7, 6, 5, 3, 0, 9];
        let polynomial = NatPolynomial::<16>::new_from_coeffs(coeffs).unwrap();

        assert_eq!(polynomial.0, 0x0000_0903_5678);

        let mut roundtrip = polynomial.iter_coeff();
        for (a, b) in roundtrip.zip(coeffs) {
            assert_eq!(a, b)
        }
    }

    #[test]
    fn combinatorics() {
        let one_plus_x = NatPolynomial::<10>::ONE + NatPolynomial::<10>::X;

        assert_eq!(one_plus_x.pow(0).0, 1);
        assert_eq!(one_plus_x.pow(1).0, 1 + 10);
        assert_eq!(one_plus_x.pow(2).0, 1 + 20 + 100);
        assert_eq!(one_plus_x.pow(3).0, 1 + 30 + 300 + 1000);
        assert_eq!(one_plus_x.pow(4).0, 1 + 40 + 600 + 4000 + 10000);

        let four_choose_n = one_plus_x.pow(4);
        assert_eq!(four_choose_n.0, 11.pow(4));
        assert!(four_choose_n.iter_coeff().eq([1, 4, 6, 4, 1]));
    }

    #[test]
    #[should_panic(expected = "iter_coeff")]
    fn careful_about_overflow() {
        let one_plus_x = NatPolynomial::<10>::ONE + NatPolynomial::<10>::X;

        let five_choose_n = one_plus_x.pow(5);
        assert_eq!(five_choose_n.0, 11.pow(5));
        assert_eq!(five_choose_n.0, 1 + 50 + 1000 + 10000 + 50000 + 100000);

        // Assertion should fail!
        assert!(five_choose_n.iter_coeff().eq([1, 5, 10, 10, 5, 1]))
    }

    #[test]
    fn degree() {
        assert_eq!(NatPolynomial::<256>::ZERO.get_degree(), None);

        assert_eq!(NatPolynomial::<256>::X.pow(0).get_degree(), Some(0));
        assert_eq!(NatPolynomial::<256>::X.pow(1).get_degree(), Some(1));
        assert_eq!(NatPolynomial::<256>::X.pow(2).get_degree(), Some(2));
        assert_eq!(NatPolynomial::<256>::X.pow(3).get_degree(), Some(3));
        assert_eq!(NatPolynomial::<256>::X.pow(4).get_degree(), Some(4));
    }

    #[test]
    fn from_raw() {
        let jennys = NatPolynomial::<10>::from_raw(8765309);
        assert_eq!(jennys.get_degree(), Some(6));
        assert_eq!(
            jennys.iter_coeff().collect::<Vec<_>>(),
            [9, 0, 3, 5, 6, 7, 8]
        );

        let jennys_hex = NatPolynomial::<16>::from_raw(0x8765309);
        assert_eq!(jennys_hex.get_degree(), Some(6));
        assert_eq!(
            jennys_hex.iter_coeff().collect::<Vec<_>>(),
            [9, 0, 3, 5, 6, 7, 8]
        );
    }

    #[test]
    fn from_iter() {
        let array = [0, 1, 2, 3, 4, 5, 6, 7];

        let from_iter = NatPolynomial::<256>::new_from_coeffs(array).unwrap();
        assert_eq!(from_iter.get_degree(), Some(7));
        assert_eq!(
            from_iter.iter_coeff().collect::<Vec<_>>(),
            [0, 1, 2, 3, 4, 5, 6, 7]
        );
    }

    #[test]
    fn largest_supported() {
        assert_eq!(NatPolynomial::<256>::LARGEST_SUPPORTED_DEGREE, 7);
    }

    #[test]
    fn from_raw_max() {
        fn parameterized<const X: u16>() {
            {
                let true_max = NatPolynomial::<X>::from_raw(u64::MAX);
                assert!(
                    true_max.get_degree().unwrap() >= NatPolynomial::<X>::LARGEST_SUPPORTED_DEGREE
                );
            }

            let max_supported = {
                let mut max_supported = NatPolynomial::<X>::LARGEST_CONSTANT_POLY;
                while max_supported.get_degree().unwrap()
                    < NatPolynomial::<X>::LARGEST_SUPPORTED_DEGREE
                {
                    max_supported.mul_x();
                    max_supported = max_supported + NatPolynomial::<X>::LARGEST_CONSTANT_POLY;
                }
                max_supported
            };

            println!("      u64::max value: {:>20}", u64::MAX);
            println!(" max_supported value: {:>20}", max_supported.0);
            println!(
                "max_supported degree: {:>20}",
                max_supported.get_degree().unwrap()
            );
        }

        parameterized::<3>();
        parameterized::<4>();
        parameterized::<5>();
        parameterized::<27>();
        parameterized::<128>();
        parameterized::<256>();
    }
}
