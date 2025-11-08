use core::fmt::Display;
use core::iter::Sum;
use core::ops::Add;
use core::ops::AddAssign;
use core::ops::Mul;

use crate::math::commutative_ring::CommutativeRing;

pub mod array;
pub mod nat;

#[allow(
    unused,
    reason = "This only really exists to demonstrate properties of the trait"
)]
mod let_x_eq_one;

struct PolynomialCoeffIterator<T: PolynomialRing>(T);

impl<T: PolynomialRing> Iterator for PolynomialCoeffIterator<T> {
    type Item = T::Over;

    fn next(&mut self) -> Option<Self::Item> {
        if self.0 == T::ZERO {
            None
        } else {
            let constant;
            (self.0, constant) = self.0.inverse_mul_x_add();
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
            let constant;
            (self.0, constant) = self.0.inverse_mul_x_add();
            let out = self.1 * T::from(constant);
            self.1 *= T::X;
            Some(out)
        }
    }
}

/// A polynomial ring. Superset of `Self::Over`.
/// Ideally closed under addition and multiplication.
///
/// Every (representable) polynomial must be able to be generated inductively
/// from `Add` and `PolynomialRing::mul_x`.
///
/// Add and Mul are allowed to panic.
///
/// Coefficients are a subset of `PolynomialRing::Over`.
/// Terms are a subset of `Self`, a product of Xs and coefficients.
#[must_use]
pub trait PolynomialRing: CommutativeRing + From<Self::Over> {
    type Over: CommutativeRing;
    const X: Self;

    /// The exponent of the highest power of X representable with Self.
    /// For all valid polynomials, `get_degree() <= MAX_EXP_X`
    const MAX_EXP_X: usize;
    /// The representation of X that can be generated from multiplying only.
    const MAX_POW_X: Self;

    fn get_constant_coeff(&self) -> Self::Over;
    fn get_constant_term(&self) -> Self;

    fn is_constant(&self) -> bool;

    /// Returns (a, b) such that ax + b = self.
    ///
    /// This is always well defined due to the trait's inductive guarantee.
    /// It should never panic.
    #[must_use]
    fn inverse_mul_x_add(&self) -> (Self, Self::Over);

    /// # Panics
    /// self * X cannot be represented.
    #[must_use]
    fn mul_x(&self) -> Self;

    // ----- Has generic implementation -----

    fn iter_coeff(&self) -> impl Iterator<Item = Self::Over> {
        PolynomialCoeffIterator(*self)
    }

    /// # Panics
    /// Default implementation panics if the polynomial cannot be represented.
    /// Implementors are allowed to panic too.
    fn new_from_coeffs(into_coeffs: impl IntoIterator<Item = Self::Over>) -> Option<Self> {
        let mut coeffs = into_coeffs.into_iter();
        if let Some(first) = coeffs.next() {
            let mut sum = Self::from(first);
            let mut power = Self::ONE;
            for coeff in coeffs {
                power *= Self::X;
                sum += power * Self::from(coeff);
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
