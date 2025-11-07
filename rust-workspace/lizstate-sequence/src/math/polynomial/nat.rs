use core::ops::Add;
use core::ops::AddAssign;
use core::ops::Mul;
use core::ops::Sub;

use crate::math::commutative_ring::CommutativeRing;
use crate::math::commutative_ring::integers_mod::NatMod;
use crate::math::polynomial::PolynomialRing;

#[derive(Debug, Clone, Copy, PartialEq, Eq, PartialOrd, Ord)]
pub struct NatPolynomial<const X: u16>(u64);

impl<const X: u16> NatPolynomial<X> {
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
}

impl<const X: u16> PolynomialRing for NatPolynomial<X> {
    type Over = u64;
    const X: Self = Self(X as u64);

    fn get_constant_coeff(&self) -> u64 {
        self.0 % (X as u64)
    }

    fn get_constant_term(&self) -> Self {
        Self(self.0 % (X as u64))
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
    use core::ops::Deref;
    use core::u64;
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
        let max = NatPolynomial::<256>::from_raw(u64::MAX);
        assert_eq!(max.get_degree(), Some(7));
        assert_eq!(max.iter_coeff().collect::<Vec<_>>(), [255; 8]);
    }
}
