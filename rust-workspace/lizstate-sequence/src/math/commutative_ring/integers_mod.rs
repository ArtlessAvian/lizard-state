use core::fmt::Debug;
use core::ops::Add;
use core::ops::AddAssign;
use core::ops::Deref;
use core::ops::Mul;
use core::ops::MulAssign;
use core::ops::Rem;

use crate::math::commutative_ring::CommutativeRing;

pub type ZModP = NatMod<251>;

/// Small numbers, intentionally wrapping.
///
/// Always between 0..MOD, exclusive.
///
/// Does NOT have multiplicative inverse sometimes.
/// 4 * x \equiv 1 (mod 10) has no solution.
#[derive(Debug, Clone, Copy, PartialEq, Eq)]
pub struct NatMod<const MOD: u8>(u8);

impl<const MOD: u8, T> From<T> for NatMod<MOD>
where
    T: From<u8> + Rem<Output = T> + TryInto<u8>,
    T::Error: Debug,
{
    fn from(value: T) -> Self {
        let modulo: T = MOD.into();
        let remainder = value % modulo;
        let as_u8 = remainder.try_into().unwrap();
        Self(as_u8)
    }
}

impl<const MOD: u8> NatMod<MOD> {
    const _NON_ZERO: () = assert!(MOD != 0);
    const CARRY: Self = Self::new_u16(u8::MAX as u16 + 1);

    pub const fn new(value: u8) -> Self {
        Self(value % MOD)
    }

    pub const fn new_u16(value: u16) -> Self {
        Self((value % MOD as u16) as u8)
    }

    pub const fn new_u32(value: u32) -> Self {
        Self((value % MOD as u32) as u8)
    }

    pub const fn new_u64(value: u64) -> Self {
        Self((value % MOD as u64) as u8)
    }
}

impl<const MOD: u8> CommutativeRing for NatMod<MOD> {
    const ZERO: Self = Self(0);
    const ONE: Self = Self(1);
}

impl<const MOD: u8> Add for NatMod<MOD> {
    type Output = Self;

    fn add(self, rhs: Self) -> Self::Output {
        let (sum_wo_carry, carry) = self.0.carrying_add(rhs.0, false);
        if !carry {
            Self::new(sum_wo_carry)
        } else {
            Self::new(sum_wo_carry) + Self::CARRY
        }
    }
}

impl<const MOD: u8> AddAssign for NatMod<MOD> {
    fn add_assign(&mut self, rhs: Self) {
        *self = *self + rhs;
    }
}

impl<const MOD: u8> Mul for NatMod<MOD> {
    type Output = Self;

    fn mul(self, rhs: Self) -> Self::Output {
        let product = self.0 as u16 * rhs.0 as u16;
        let remainder = product % MOD as u16;
        Self::new_u16(remainder)
    }
}

impl<const MOD: u8> MulAssign for NatMod<MOD> {
    fn mul_assign(&mut self, rhs: Self) {
        *self = *self * rhs;
    }
}

#[cfg(test)]
mod tests {
    use crate::math::commutative_ring::integers_mod::NatMod;
    use crate::math::commutative_ring::integers_mod::ZModP;

    #[test]
    fn commutative() {
        let a = ZModP::new_u16(100);
        let b = ZModP::new_u16(200);
        assert_eq!(a + b, b + a);
        assert_eq!(a * b, b * a);

        let a = NatMod::<13>::new_u16(100);
        let b = NatMod::<13>::new_u16(200);
        assert_eq!(a + b, b + a);
        assert_eq!(a * b, b * a);
    }

    #[test]
    fn distributive() {
        let a = ZModP::new_u16(100);
        let b = ZModP::new_u16(200);
        let c = ZModP::new_u16(300);
        assert_eq!(a * (b + c), a * b + a * c);

        let a = NatMod::<7>::new_u16(100);
        let b = NatMod::<7>::new_u16(200);
        let c = NatMod::<7>::new_u16(300);
        assert_eq!(a * (b + c), a * b + a * c);
    }
}
