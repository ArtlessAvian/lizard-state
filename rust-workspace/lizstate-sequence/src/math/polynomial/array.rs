use core::array::from_fn;
use core::fmt::Display;
use core::iter::Sum;
use core::ops::Add;
use core::ops::AddAssign;
use core::ops::Mul;
use core::ops::MulAssign;
#[cfg(test)]
use std::dbg;
#[cfg(test)]
use std::println;

use crate::math::commutative_ring::CommutativeRing;
use crate::math::commutative_ring::integers_mod::NatMod;
use crate::math::polynomial::PolynomialRing;
use crate::math::polynomial::nat::NatPolynomial;

type DegreeSevenPolynomial = ArrayPolynomial<8>;

#[derive(Debug, Clone, Copy, PartialEq, Eq, PartialOrd, Ord)]
pub struct ArrayPolynomial<const LEN: usize>([u8; LEN]);

impl<const LEN: usize> ArrayPolynomial<LEN> {
    pub const fn new_from_array(array: [u8; LEN]) -> Self {
        Self(array)
    }

    pub const fn new_from_nat_poly<const X: u16>(nat: &NatPolynomial<X>) -> Self {
        let mut array = [0; LEN];

        let mut rem = nat.leak();
        let mut i = 0;
        while rem > 0 {
            array[i] = (rem % X as u64) as u8;
            rem /= X as u64;
            i += 1;
        }

        Self(array)
    }

    pub(crate) const fn leak(&self) -> [u8; LEN] {
        self.0
    }
}

impl<const LEN: usize> Display for ArrayPolynomial<LEN> {
    fn fmt(&self, f: &mut core::fmt::Formatter<'_>) -> core::fmt::Result {
        write!(f, "(")?;
        for (i, coeff) in self.0.iter().enumerate() {
            if i != 0 {
                write!(f, " + ")?;
            }
            write!(f, "{}x^{}", coeff, i)?;
        }
        write!(f, ")")?;
        Ok(())
    }
}

impl<const LEN: usize> PolynomialRing for ArrayPolynomial<LEN> {
    type Over = u8;
    const X: Self = {
        let mut array = [0; LEN];
        array[1] = Self::Over::ONE;
        Self(array)
    };

    fn get_constant_coeff(&self) -> u8 {
        self.0[0]
    }

    fn get_constant_term(&self) -> Self {
        Self::ONE * self.get_constant_coeff()
    }

    fn is_constant(&self) -> bool {
        self.0.iter().skip(1).all(|x| *x == 0)
    }

    fn drop_constant_and_divide_x(&self) -> Self {
        let mut out = *self;
        out.0.copy_within(1..(LEN), 0);
        out.0[LEN - 1] = 0;
        out
    }

    fn mul_x(&self) -> Self {
        let mut out = *self;
        if (out.0[LEN - 1] != 0) {
            panic!("Attempted mul_x with overflow!");
        }
        out.0.copy_within(0..(LEN - 1), 1);
        out.0[0] = 0;
        out
    }

    fn iter_coeff(&self) -> impl Iterator<Item = Self::Over> {
        self.0.into_iter()
    }
}

impl<const LEN: usize> TryFrom<u8> for ArrayPolynomial<LEN> {
    type Error = ();

    fn try_from(value: u8) -> Result<Self, Self::Error> {
        let mut out = Self::ZERO;
        out.0[0] = value;
        Ok(out)
    }
}

impl<const LEN: usize> CommutativeRing for ArrayPolynomial<LEN> {
    const ZERO: Self = Self([0; LEN]);
    const ONE: Self = {
        let mut out = Self::ZERO;
        out.0[0] = 1;
        out
    };
}

impl<const LEN: usize> Add<u8> for ArrayPolynomial<LEN> {
    type Output = Self;

    fn add(self, rhs: u8) -> Self::Output {
        let mut out = self;
        out.0[0] += rhs;
        out
    }
}

impl<const LEN: usize> Mul<u8> for ArrayPolynomial<LEN> {
    type Output = Self;

    fn mul(self, rhs: u8) -> Self::Output {
        let mut out = self;
        out.0.iter_mut().for_each(|x| *x *= rhs);
        out
    }
}

impl<const LEN: usize> Add for ArrayPolynomial<LEN> {
    type Output = Self;

    fn add(self, rhs: Self) -> Self::Output {
        let mut out = Self::ZERO;
        for (a, (b, c)) in out.0.iter_mut().zip(self.0.iter().zip(rhs.0.iter())) {
            *a = b + c;
        }
        out
    }
}

impl<const LEN: usize> AddAssign for ArrayPolynomial<LEN> {
    fn add_assign(&mut self, rhs: Self) {
        *self = *self + rhs
    }
}

impl<const LEN: usize> Mul for ArrayPolynomial<LEN> {
    type Output = Self;

    fn mul(self, rhs: Self) -> Self::Output {
        if self == Self::ZERO || rhs == Self::ZERO {
            Self::ZERO
        } else if self == Self::ONE {
            rhs
        } else if rhs == Self::ONE {
            self
        } else {
            #[cfg(test)]
            {
                println!("{self} * {rhs}")
            }

            let mut sum = Self::ZERO;

            for (i, coeff) in self.0.iter().enumerate() {
                #[cfg(test)]
                {
                    println!("{i} {coeff}")
                }

                if *coeff != 0 {
                    let mut shifted = rhs.mul(*coeff);
                    for _ in 0..i {
                        shifted = shifted.mul_x();
                    }
                    let old = sum;
                    sum += shifted;

                    #[cfg(test)]
                    {
                        println!("  {old}");
                        println!("+ {shifted}");
                        println!("= {sum}");
                        println!()
                    }
                }
            }

            sum
        }
    }
}

impl<const LEN: usize> MulAssign for ArrayPolynomial<LEN> {
    fn mul_assign(&mut self, rhs: Self) {
        *self = *self * rhs
    }
}

#[cfg(test)]
mod tests {
    use core::ops::Deref;

    use crate::math::commutative_ring::CommutativeRing;
    use crate::math::commutative_ring::integers_mod::NatMod;
    use crate::math::polynomial::PolynomialRing;
    use crate::math::polynomial::array::DegreeSevenPolynomial;

    #[test]
    fn consts() {
        assert_eq!(DegreeSevenPolynomial::ZERO.0, [0, 0, 0, 0, 0, 0, 0, 0]);
        assert_eq!(DegreeSevenPolynomial::ONE.0, [1, 0, 0, 0, 0, 0, 0, 0]);
        assert_eq!(DegreeSevenPolynomial::X.0, [0, 1, 0, 0, 0, 0, 0, 0]);
    }

    #[test]
    fn array_round_trip() {
        let coeffs = [8, 7, 6, 5, 3, 0, 9];
        let polynomial = DegreeSevenPolynomial::new_from_coeffs(coeffs).unwrap();

        assert_eq!(polynomial.0, [8, 7, 6, 5, 3, 0, 9, 0]);

        let mut roundtrip = polynomial.iter_coeff();
        for (a, b) in roundtrip.zip(coeffs) {
            assert_eq!(a, b)
        }
    }

    #[test]
    fn combinatorics() {
        let one_plus_x = DegreeSevenPolynomial::ONE + DegreeSevenPolynomial::X;

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
    #[should_panic(expected = "overflow")]
    fn mul_overflow() {
        DegreeSevenPolynomial::X.pow(8);
    }

    #[test]
    #[should_panic(expected = "overflow")]
    fn mul_x_overflow() {
        DegreeSevenPolynomial::X.pow(7).mul_x();
    }
}
