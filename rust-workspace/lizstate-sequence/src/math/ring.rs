use core::fmt::Debug;

pub mod fixed;
pub mod fraction;

/// A superset of u8, "closed" under addition and multiplication.
///
/// Implementors are allowed to panic on addition or multiplication,
/// since we cannot represent infinitely many values.
///
/// We can define a power series on a ring.
/// (An infinite power series might not necessarily converge.)
///
/// We don't really care if our ring is a field, having subtraction and division.
/// We do care about finding inverses for `mul_add(a, b, c: Nat) = ab + c`.
/// (If they exist in our representation.)
pub trait Ring:
    Copy
    + From<u8>
    + TryInto<u8>
    + core::ops::Add<Output = Self>
    + core::ops::Sub<Output = Self>
    + core::ops::Mul<Output = Self>
    + core::cmp::PartialEq
    + core::cmp::PartialOrd
{
    const ZERO: Self;
    const ONE: Self;

    fn add_u8(self, rhs: u8) -> Self {
        self + Self::from(rhs)
    }

    fn mul_u8(self, rhs: u8) -> Self {
        self * Self::from(rhs)
    }

    /// If there exist `a * rhs + c == self`, returns (a, b) such that b is the largest natural number.
    ///
    /// If self is a power series of rhs, and `self = P(rhs) = P'(rhs) * rhs + c`, returns (P'(rhs) and c).
    fn inv_mul_whole_add(self, rhs: Self) -> Option<(Self, u8)>;

    fn pow_u8(self, mut exp: u8) -> Self {
        let mut product = Self::ONE;
        let mut pow = self;
        while exp > 0 {
            if exp & 1 != 0 {
                product = product * pow;
            }
            pow = pow * pow;
            exp >>= 1;
        }
        product
    }

    /// Returns the smallest u32 `exp` such that `self <= base.pow(exp)`.
    ///
    /// # Panics
    /// * The function may panic when `base` is less than 2,
    ///   or when the exponent overflows a u8.
    fn bounding_exponent(self, base: u8) -> u8 {
        if base < 2 {
            panic!()
        }
        if self <= Self::ONE {
            return 0;
        }

        let base = Self::from(base);
        if self <= base {
            return 1;
        }

        let mut exponent = 1;
        let mut power = base;
        {
            let mut previous_exponent = 1;
            let mut previous_pow = base;
            while power * previous_pow < self {
                // power *= 2 grows at the rate of 2.
                // this grows at the rate of 1.6 i guess.
                (previous_exponent, exponent) = (exponent, previous_exponent + exponent);
                (previous_pow, power) = (power, previous_pow * power);
            }
        }

        // Cover the remaining distance.
        while power < self {
            exponent += 1;
            power = power * base;
        }

        exponent
    }

    /// Returns the smallest value `value` such that `self <= value = base^n` for some nonnegative `n`.
    ///
    /// # Panics
    /// * `base` is less than 2.
    fn bounding_power(self, base: u8) -> Self {
        Self::from(base).pow_u8(self.bounding_exponent(base))
    }
}

impl Ring for u64 {
    const ZERO: u64 = 0u64;
    const ONE: u64 = 1u64;

    fn mul_u8(self, rhs: u8) -> Self {
        self * rhs as u64
    }

    fn inv_mul_whole_add(self, rhs: Self) -> Option<(Self, u8)> {
        Some((self / rhs, (self % rhs) as u8))
    }

    fn pow_u8(self, exp: u8) -> Self {
        self.pow(exp as u32)
    }

    fn bounding_exponent(self, base: u8) -> u8 {
        if self == 0 || self == 1 {
            0
        } else if self <= Self::from(base) {
            1
        } else {
            // This works, but behaves strangely for 2.log(3).
            // (2 * 3 as u64 - 1).ilog(3 as u64)
            // (5).ilog(3 as u64)
            (self * base as u64 - 1).ilog(base as u64) as u8
        }
        // returned value is always <= 64
    }
}

#[cfg(test)]
mod tests {
    use crate::math::ring::Ring;

    #[test]
    fn pow() {
        for exp in 0..10u8 {
            for value in (0..10u8).map(u64::from) {
                assert_eq!(value.pow_u8(exp), value.pow(exp as u32), "{value}, {exp}");
            }
        }
    }

    #[test]
    fn bounding_exponent() {
        for base in 2..10u8 {
            for value in (0..100u8).map(u64::from) {
                let exponent = value.bounding_exponent(base);
                let power = (base as u64).pow(exponent as u32);

                assert!(
                    value <= power,
                    "assertion `{value} <= {base}.pow({exponent}) == {power}` failed"
                );

                if exponent > 0 {
                    let previous_power = (base as u64).pow(exponent as u32 - 1);
                    assert!(
                        previous_power <= value,
                        "assertion `smallest exponent` failed",
                    )
                }
            }
        }
    }

    #[test]
    fn bounding_power() {
        for base in 2..10u8 {
            for value in (0..100u8).map(u64::from) {
                let leq = value.bounding_power(base);
                assert!(value <= leq, "assertion `{value} <= {leq}` failed")
            }
        }
    }
}
