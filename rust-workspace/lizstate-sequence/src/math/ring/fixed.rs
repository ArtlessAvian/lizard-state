use core::cmp::Ordering;
use core::fmt::Display;
use core::ops::Add;
use core::ops::Div;
use core::ops::Mul;
use core::ops::Rem;
use core::ops::Sub;

use crate::math::ring::Ring;

// Nonnegative rational number with a known denominator.
#[derive(Debug, Clone, Copy, Default, PartialEq, Eq, PartialOrd, Ord)]
pub struct FixedFraction<const DENOM: u64>(pub u64);

impl<const DENOM: u64> Display for FixedFraction<DENOM> {
    fn fmt(&self, f: &mut core::fmt::Formatter<'_>) -> core::fmt::Result {
        write!(f, "({} / {DENOM})", self.0)
    }
}

impl<const DENOM: u64> FixedFraction<DENOM> {
    fn floor(&self) -> u8 {
        (self.0 / DENOM) as u8
    }
}

impl<const DENOM: u64> Ring for FixedFraction<DENOM> {
    const ZERO: Self = Self(0);
    const ONE: Self = Self(DENOM);

    fn inv_mul_whole_add(self, rhs: Self) -> Option<(Self, u8)> {
        let remainder = self.floor();
        let product = self - Self::from(remainder);
        let factor = (|| {
            // the dumb way
            for i in 0..(DENOM * 10) {
                let attempt = FixedFraction(i);
                if (attempt.0 * rhs.0).is_multiple_of(DENOM)
                    && attempt.0 * rhs.0 / DENOM == product.0
                {
                    return Some(attempt);
                }
            }
            None
        })();
        factor.map(|x| (x, remainder))
    }
}

impl<const DENOM: u64> From<u8> for FixedFraction<DENOM> {
    fn from(value: u8) -> Self {
        Self(DENOM * value as u64)
    }
}

impl<const DENOM: u64> TryFrom<FixedFraction<DENOM>> for u8 {
    type Error = ();

    fn try_from(value: FixedFraction<DENOM>) -> Result<Self, Self::Error> {
        if value.0.is_multiple_of(DENOM) {
            let val = value.0 / DENOM;
            u8::try_from(val).map_err(|_| ())
        } else {
            Err(())
        }
    }
}

impl<const DENOM: u64> Add for FixedFraction<DENOM> {
    type Output = Self;

    fn add(self, rhs: Self) -> Self::Output {
        Self(self.0 + rhs.0)
    }
}

impl<const DENOM: u64> Sub for FixedFraction<DENOM> {
    type Output = Self;

    fn sub(self, rhs: Self) -> Self::Output {
        Self(self.0 - rhs.0)
    }
}

impl<const DENOM: u64> Mul for FixedFraction<DENOM> {
    type Output = Self;

    fn mul(self, rhs: Self) -> Self::Output {
        if (self.0 * rhs.0).is_multiple_of(DENOM) {
            Self(self.0 * rhs.0 / DENOM)
        } else {
            panic!("Cannot represent with fixed denominator.")
        }
    }
}

impl<const DENOM: u64> PartialEq<u8> for FixedFraction<DENOM> {
    fn eq(&self, other: &u8) -> bool {
        if self.0.is_multiple_of(DENOM) {
            self.0 / DENOM == (*other) as u64
        } else {
            false
        }
    }
}

impl<const DENOM: u64> PartialEq<FixedFraction<DENOM>> for u8 {
    fn eq(&self, other: &FixedFraction<DENOM>) -> bool {
        other.eq(self)
    }
}

impl<const DENOM: u64> PartialOrd<u8> for FixedFraction<DENOM> {
    fn partial_cmp(&self, other: &u8) -> Option<Ordering> {
        if self.eq(other) {
            Some(Ordering::Equal)
        } else {
            Some(match (self.0 / DENOM).cmp(&(*other as u64)) {
                Ordering::Less => Ordering::Less,
                Ordering::Equal => Ordering::Greater,
                Ordering::Greater => Ordering::Greater,
            })
        }
    }
}

impl<const DENOM: u64> PartialOrd<FixedFraction<DENOM>> for u8 {
    fn partial_cmp(&self, other: &FixedFraction<DENOM>) -> Option<Ordering> {
        other.partial_cmp(self).map(Ordering::reverse)
    }
}
