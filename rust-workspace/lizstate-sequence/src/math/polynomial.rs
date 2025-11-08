use core::fmt::Display;
use core::iter::Sum;
use core::ops::Add;
use core::ops::AddAssign;
use core::ops::Mul;

use crate::math::commutative_ring::CommutativeRing;
use crate::math::commutative_ring::integers_mod::NatMod;

pub mod array;
pub mod nat;

struct PolynomialCoeffIterator<T: PolynomialRing>(T);

impl<T: PolynomialRing> Iterator for PolynomialCoeffIterator<T> {
    type Item = T::Over;

    fn next(&mut self) -> Option<Self::Item> {
        if self.0 == T::ZERO {
            None
        } else {
            let constant = self.0.get_constant_coeff();
            self.0.drop_constant_and_divide_x();
            Some(constant)
        }
    }
}

struct PolynomialTermIterator<T: PolynomialRing>(T, T);

impl<T: PolynomialRing> Iterator for PolynomialTermIterator<T> {
    type Item = T;

    fn next(&mut self) -> Option<Self::Item> {
        if self.0 == T::ZERO {
            None
        } else {
            let constant = self.0.get_constant_term();
            let out = constant * self.1;

            self.0.drop_constant_and_divide_x();
            self.1 = self.1 * T::X;
            Some(out)
        }
    }
}

/// A polynomial ring. Superset of `Self::Over`.
/// Ideally closed under addition and multiplication.
///
/// Every (representable) polynomial must be able to be generated inductively
/// from `Add<PolynomialRing::Over>` and `PolynomialRing::mul_x`.
///
/// Add and Mul are allowed to panic.
///
/// Coefficients are a subset of `PolynomialRing::Over`.
/// Terms are a subset of `Self`, a product of Xs and coefficients.
pub trait PolynomialRing: CommutativeRing + TryFrom<Self::Over> {
    type Over: CommutativeRing;
    const X: Self;

    fn get_constant_coeff(&self) -> Self::Over;
    fn get_constant_term(&self) -> Self;

    fn is_constant(&self) -> bool;

    /// Finds the inverse to mul_x, sorta.
    ///
    /// This function should never panic due to the trait's inductive guarantee.
    fn drop_constant_and_divide_x(&mut self);

    /// # Panics
    /// self * X cannot be represented.
    fn mul_x(&mut self);

    fn iter_coeff(&self) -> impl Iterator<Item = Self::Over>;

    // ----- Has generic implementation -----

    /// # Panics
    /// Default implementation panics if the polynomial cannot be represented.
    /// Implementors are allowed to panic too.
    fn new_from_coeffs(into_coeffs: impl IntoIterator<Item = Self::Over>) -> Option<Self> {
        let mut coeffs = into_coeffs.into_iter();
        if let Some(first) = coeffs.next() {
            let mut sum = Self::try_from(first).ok()?;
            let mut power = Self::ONE;
            for coeff in coeffs {
                power = power * Self::X;
                sum = sum + power * Self::try_from(coeff).ok()?;
            }
            Some(sum)
        } else {
            Some(Self::ZERO)
        }
    }

    fn iter_terms(&self) -> impl Iterator<Item = Self> {
        PolynomialTermIterator(*self, Self::ONE)
    }

    fn get_leading_coeff(&self) -> Option<Self::Over> {
        self.iter_coeff().last()
    }

    fn get_degree(&self) -> Option<usize> {
        if *self == Self::ZERO {
            None
        } else {
            Some(self.iter_coeff().count())
        }
    }
}
