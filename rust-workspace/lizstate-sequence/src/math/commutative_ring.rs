use core::ops::Add;
use core::ops::Mul;

pub mod integers_mod;

/// A set with commutative addition and multiplication.
pub trait CommutativeRing: Copy + Add<Output = Self> + Mul<Output = Self> + PartialEq {
    const ZERO: Self;
    const ONE: Self;

    fn pow(self, mut exp: u8) -> Self {
        let mut product = Self::ONE;
        let mut pow = self;

        if exp & 1 != 0 {
            product = product * pow;
            exp &= !1;
        }

        while exp > 0 {
            pow = pow * pow;
            exp >>= 1;
            if exp & 1 != 0 {
                product = product * pow;
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
