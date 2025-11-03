use core::iter::Sum;
use core::ops::Add;
use core::ops::AddAssign;
use core::ops::Mul;

use crate::math::commutative_ring::CommutativeRing;
use crate::math::commutative_ring::integers_mod::NatMod;
use crate::math::polynomial::PolynomialRing;

#[derive(Debug, Clone, Copy, PartialEq, Eq, PartialOrd, Ord)]
pub struct ArrayPolynomial([u8; 8]);

impl PolynomialRing for ArrayPolynomial {
    type Over = u8;
    const X: Self = Self([0, 1, 0, 0, 0, 0, 0, 0]);

    fn get_constant_coeff(&self) -> u8 {
        self.0[0]
    }

    fn get_constant_term(&self) -> Self {
        Self::ONE * self.get_constant_coeff()
    }

    fn is_constant(&self) -> bool {
        self.0.iter().skip(1).all(|x| *x == 0)
    }

    fn drop_constant_and_divide_x(&mut self) {
        for i in 0..7 {
            self.0[i] = self.0[i + 1];
        }
        self.0[7] = 0;
    }

    fn mul_x(&mut self) {
        for i in (1..=7).rev() {
            self.0[i] = self.0[i - 1];
        }
        self.0[0] = 0;
    }
}

impl TryFrom<u8> for ArrayPolynomial {
    type Error = ();

    fn try_from(value: u8) -> Result<Self, Self::Error> {
        Ok(Self([value, 0, 0, 0, 0, 0, 0, 0]))
    }
}

impl CommutativeRing for ArrayPolynomial {
    const ZERO: Self = Self([0; 8]);
    const ONE: Self = Self([1, 0, 0, 0, 0, 0, 0, 0]);
}

impl Add<u8> for ArrayPolynomial {
    type Output = Self;

    fn add(self, rhs: u8) -> Self::Output {
        let mut out = self;
        out.0[0] += rhs;
        out
    }
}

impl Mul<u8> for ArrayPolynomial {
    type Output = Self;

    fn mul(self, rhs: u8) -> Self::Output {
        let mut out = self;
        out.0.iter_mut().map(|x| *x *= rhs);
        out
    }
}

impl Add for ArrayPolynomial {
    type Output = Self;

    fn add(self, rhs: Self) -> Self::Output {
        let mut out = Self::ZERO;
        for (a, (b, c)) in out.0.iter_mut().zip(self.0.iter().zip(rhs.0.iter())) {
            *a = b + c;
        }
        out
    }
}

impl Mul for ArrayPolynomial {
    type Output = Self;

    fn mul(self, rhs: Self) -> Self::Output {
        let mut sum = [0; 8];
        for i in 0..8 {
            for j in 0..8 {
                if self.0[i] != 0 && rhs.0[j] != 0 {
                    sum[i + j] += self.0[i] * rhs.0[j];
                }
            }
        }
        Self(sum)
    }
}

#[cfg(test)]
mod tests {
    use core::ops::Deref;

    use crate::math::commutative_ring::CommutativeRing;
    use crate::math::commutative_ring::integers_mod::NatMod;
    use crate::math::polynomial::PolynomialRing;
    use crate::math::polynomial::array::ArrayPolynomial;

    #[test]
    fn consts() {
        assert_eq!(ArrayPolynomial::ZERO.0, [0, 0, 0, 0, 0, 0, 0, 0]);
        assert_eq!(ArrayPolynomial::ONE.0, [1, 0, 0, 0, 0, 0, 0, 0]);
        assert_eq!(ArrayPolynomial::X.0, [0, 1, 0, 0, 0, 0, 0, 0]);
    }

    #[test]
    fn array_round_trip() {
        let coeffs = [8, 7, 6, 5, 3, 0, 9];
        let polynomial = ArrayPolynomial::new_from_coeffs(coeffs).unwrap();

        assert_eq!(polynomial.0, [8, 7, 6, 5, 3, 0, 9, 0]);

        let mut roundtrip = polynomial.iter_coeff();
        for (a, b) in roundtrip.zip(coeffs) {
            assert_eq!(a, b)
        }
    }

    #[test]
    fn combinatorics() {
        let one_plus_x = ArrayPolynomial::ONE + ArrayPolynomial::X;

        assert_eq!(one_plus_x.pow(0).0, [1, 0, 0, 0, 0, 0, 0, 0]);
        assert_eq!(one_plus_x.pow(1).0, [1, 1, 0, 0, 0, 0, 0, 0]);
        assert_eq!(one_plus_x.pow(2).0, [1, 2, 1, 0, 0, 0, 0, 0]);
        assert_eq!(one_plus_x.pow(3).0, [1, 3, 3, 1, 0, 0, 0, 0]);
        assert_eq!(one_plus_x.pow(4).0, [1, 4, 6, 4, 1, 0, 0, 0]);
        assert_eq!(one_plus_x.pow(5).0, [1, 5, 10, 10, 5, 1, 0, 0]);
        assert_eq!(one_plus_x.pow(6).0, [1, 6, 15, 20, 15, 6, 1, 0]);
        assert_eq!(one_plus_x.pow(7).0, [1, 7, 21, 35, 35, 21, 7, 1]);
    }

    #[test]
    #[should_panic(expected = "out of bounds")]
    fn careful_about_overflow() {
        ArrayPolynomial::X.pow(8);
    }
}
