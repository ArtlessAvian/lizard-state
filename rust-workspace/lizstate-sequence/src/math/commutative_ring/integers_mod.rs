use core::ops::Add;
use core::ops::Deref;
use core::ops::Mul;

use crate::math::commutative_ring::CommutativeRing;

#[derive(Debug, Clone, Copy, PartialEq, Eq)]
pub struct NatMod<const MOD: u8>(u8);

impl<const MOD: u8> Deref for NatMod<MOD> {
    type Target = u8;

    fn deref(&self) -> &Self::Target {
        &self.0
    }
}

impl<const MOD: u8> NatMod<MOD> {
    pub fn new(u8: u8) -> Self {
        Self(u8 % MOD)
    }
}

impl<const MOD: u8> CommutativeRing for NatMod<MOD> {
    const ZERO: Self = Self(0);
    const ONE: Self = Self(1);
}

impl<const MOD: u8> From<u8> for NatMod<MOD> {
    fn from(value: u8) -> Self {
        Self::new(value)
    }
}

impl<const MOD: u8> Add for NatMod<MOD> {
    type Output = Self;

    fn add(self, rhs: Self) -> Self::Output {
        Self::new(self.0 + rhs.0)
    }
}

impl<const MOD: u8> Mul for NatMod<MOD> {
    type Output = Self;

    fn mul(self, rhs: Self) -> Self::Output {
        Self::new(self.0 * rhs.0)
    }
}
