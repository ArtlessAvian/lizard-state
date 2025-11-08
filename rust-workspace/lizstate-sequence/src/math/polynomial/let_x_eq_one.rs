use core::ops::Add;
use core::ops::AddAssign;
use core::ops::Mul;
use core::ops::MulAssign;

use crate::math::commutative_ring::CommutativeRing;
use crate::math::polynomial::PolynomialRing;

#[derive(Debug, Clone, Copy, PartialEq, Eq, PartialOrd, Ord)]
struct LetXEqOne<T: CommutativeRing>(T);

impl<T: CommutativeRing> PolynomialRing for LetXEqOne<T> {
    type Over = T;
    const X: Self = Self::ONE;
    const MAX_POW_X: Self = Self::ONE;
    const MAX_EXP_X: usize = 0;

    fn get_constant_coeff(&self) -> Self::Over {
        self.0
    }

    fn get_constant_term(&self) -> Self {
        *self
    }

    fn is_constant(&self) -> bool {
        true
    }

    fn inverse_mul_x_add(&self) -> (Self, Self::Over) {
        (Self::ZERO, self.0)
    }

    fn mul_x(&self) -> Self {
        *self
    }

    fn iter_coeff(&self) -> impl Iterator<Item = Self::Over> {
        [self.0].into_iter()
    }
}

impl<T: CommutativeRing> From<T> for LetXEqOne<T> {
    fn from(value: T) -> Self {
        Self(value)
    }
}

impl<T: CommutativeRing> CommutativeRing for LetXEqOne<T> {
    const ZERO: Self = Self(T::ZERO);
    const ONE: Self = Self(T::ONE);
}

impl<T: CommutativeRing> Add<T> for LetXEqOne<T> {
    type Output = Self;

    fn add(self, rhs: T) -> Self::Output {
        Self(self.0 + rhs)
    }
}

impl<T: CommutativeRing> Mul<T> for LetXEqOne<T> {
    type Output = Self;

    fn mul(self, rhs: T) -> Self::Output {
        Self(self.0 * rhs)
    }
}

impl<T: CommutativeRing> Add for LetXEqOne<T> {
    type Output = Self;

    fn add(self, rhs: Self) -> Self::Output {
        Self(self.0 + rhs.0)
    }
}

impl<T: CommutativeRing> AddAssign for LetXEqOne<T> {
    fn add_assign(&mut self, rhs: Self) {
        *self = *self + rhs
    }
}

impl<T: CommutativeRing> Mul for LetXEqOne<T> {
    type Output = Self;

    fn mul(self, rhs: Self) -> Self::Output {
        Self(self.0 * rhs.0)
    }
}

impl<T: CommutativeRing> MulAssign for LetXEqOne<T> {
    fn mul_assign(&mut self, rhs: Self) {
        *self = *self * rhs
    }
}
