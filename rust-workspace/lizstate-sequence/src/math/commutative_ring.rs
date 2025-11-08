use core::ops::Add;
use core::ops::AddAssign;
use core::ops::Index;
use core::ops::Mul;
use core::ops::MulAssign;

pub mod integers_mod;

/// A set with commutative addition and multiplication.
///
/// Add and mul are allowed to panic, wrap, or become invalid.
///
/// Addition has an inverse, but we mostly ignore it, like a semi-ring.
pub trait CommutativeRing:
    Copy + Add<Output = Self> + AddAssign + Mul<Output = Self> + MulAssign + PartialEq
{
    /// Additive identity
    const ZERO: Self;
    /// Multiplicative identity
    const ONE: Self;

    fn pow(self, mut exp: u8) -> Self {
        let mut product = Self::ONE;
        let mut pow = self;

        if exp & 1 != 0 {
            product *= pow;
            exp &= !1;
        }

        while exp > 0 {
            pow = pow * pow;
            exp >>= 1;
            if exp & 1 != 0 {
                product *= pow;
                exp &= !1;
            }
        }
        product
    }
}

impl CommutativeRing for u8 {
    const ZERO: Self = 0;
    const ONE: Self = 1;
}

impl CommutativeRing for u64 {
    const ZERO: Self = 0;
    const ONE: Self = 1;
}

impl CommutativeRing for f64 {
    const ZERO: Self = 0.0;
    const ONE: Self = 1.0;
}
