use core::cmp::Ordering;
use core::fmt::Display;
use core::ops::Add;
use core::ops::Div;
use core::ops::Mul;
use core::ops::Rem;
use core::ops::Sub;

use crate::math::ring::Ring;

/// Nonnegative rational numbers.
///
/// There are multiple representations of the same number.
#[derive(Debug, Clone, Copy, Default)]
pub struct Fraction(pub u64, pub u64);

impl Display for Fraction {
    fn fmt(&self, f: &mut core::fmt::Formatter<'_>) -> core::fmt::Result {
        write!(f, "({} / {})", self.0, self.1)
    }
}

impl Fraction {
    pub const fn gcd(a: u64, b: u64) -> u64 {
        if a > b {
            Self::gcd(b, a)
        } else if a == 0 {
            b
        } else {
            Self::gcd(b % a, a)
        }
    }

    pub const fn recip(self) -> Self {
        Self(self.1, self.0)
    }

    pub fn floor(self) -> u8 {
        (self.0 / self.1) as u8
    }
}

impl Ring for Fraction {
    const ZERO: Self = Fraction(0, 1);
    const ONE: Self = Fraction(1, 1);

    fn inv_mul_whole_add(self, rhs: Self) -> Option<(Self, u8)> {
        let remainder = self.floor();
        let product = self - Self::from(remainder);
        let factor = product * rhs.recip();
        Some((factor, remainder))
    }
}

impl From<u8> for Fraction {
    fn from(value: u8) -> Self {
        Self(value as u64, 1)
    }
}

impl TryFrom<Fraction> for u8 {
    type Error = ();

    fn try_from(value: Fraction) -> Result<Self, Self::Error> {
        if value.0.is_multiple_of(value.1) {
            Ok((value.0 / value.1) as u8)
        } else {
            Err(())
        }
    }
}

impl Add for Fraction {
    type Output = Self;

    fn add(self, rhs: Self) -> Self::Output {
        let gcd = Self::gcd(self.1, rhs.1);

        let denom = self.1 * rhs.1 / gcd;
        let left = self.0 * denom / self.1;
        let right = rhs.0 * denom / rhs.1;

        Fraction(left + right, denom)
    }
}

impl Sub for Fraction {
    type Output = Self;

    fn sub(self, rhs: Self) -> Self::Output {
        let gcd = Self::gcd(self.1, rhs.1);

        let denom = self.1 * rhs.1 / gcd;
        let left = self.0 * denom / self.1;
        let right = rhs.0 * denom / rhs.1;

        Fraction(left - right, denom)
    }
}

impl Mul for Fraction {
    type Output = Self;

    fn mul(self, rhs: Self) -> Self::Output {
        let num = self.0 * rhs.0;
        let denom = self.1 * rhs.1;
        let factor = Self::gcd(num, denom);
        Fraction(num / factor, denom / factor)
    }
}

impl PartialEq for Fraction {
    fn eq(&self, other: &Self) -> bool {
        self.0 * other.1 == other.0 * self.1
    }
}

impl Eq for Fraction {}

impl PartialOrd for Fraction {
    fn partial_cmp(&self, other: &Self) -> Option<Ordering> {
        Some(self.cmp(other))
    }
}

impl Ord for Fraction {
    fn cmp(&self, other: &Self) -> Ordering {
        (self.0 * other.1).cmp(&(other.0 * self.1))
    }
}

impl PartialEq<u8> for Fraction {
    fn eq(&self, other: &u8) -> bool {
        if self.0.is_multiple_of(self.1) {
            self.0 / self.1 == (*other) as u64
        } else {
            false
        }
    }
}

impl PartialEq<Fraction> for u8 {
    fn eq(&self, other: &Fraction) -> bool {
        other.eq(self)
    }
}

impl PartialOrd<u8> for Fraction {
    fn partial_cmp(&self, other: &u8) -> Option<Ordering> {
        if self.eq(other) {
            Some(Ordering::Equal)
        } else {
            Some(match (self.0 / self.1).cmp(&(*other as u64)) {
                Ordering::Less => Ordering::Less,
                Ordering::Equal => Ordering::Greater,
                Ordering::Greater => Ordering::Greater,
            })
        }
    }
}

impl PartialOrd<Fraction> for u8 {
    fn partial_cmp(&self, other: &Fraction) -> Option<Ordering> {
        other.partial_cmp(self).map(Ordering::reverse)
    }
}
