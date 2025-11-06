#![allow(unused, reason = "WIP")]

use crate::math::commutative_ring::CommutativeRing;
use crate::math::polynomial::PolynomialRing;
use crate::math::polynomial::nat::NatPolynomial;

#[derive(Debug, Clone, Copy, PartialEq, Eq)]
pub struct DequeFull;
#[derive(Debug, Clone, Copy, PartialEq, Eq)]
pub struct DequeEmpty;

#[derive(Debug, Clone, Copy)]
struct Deque<const BOUND: u8, const CAPACITY: usize = 8>(NatPolynomial<BOUND>);

impl<const BOUND: u8, const CAPACITY: usize> Deque<BOUND, CAPACITY> {
    pub fn new_empty() -> Self {
        Deque(NatPolynomial::ONE)
    }

    pub fn is_empty(&self) -> bool {
        self.0 == NatPolynomial::ONE
    }

    pub fn is_full(&self) -> bool {
        self.0.get_degree() == CAPACITY + 1
    }

    pub fn push_low(&mut self, el: u8) -> Result<(), DequeFull> {
        if self.is_full() {
            Err(DequeFull)
        } else {
            self.0.mul_x();
            self.0 = self.0 + NatPolynomial::ONE * el;
            Ok(())
        }
    }

    pub fn pop_low(&mut self) -> Result<u8, DequeEmpty> {
        if self.is_empty() {
            Err(DequeEmpty)
        } else {
            let out = self.0.get_constant_coeff() as u8;
            self.0.drop_constant_and_divide_x();
            Ok(out)
        }
    }

    pub fn push_high(&mut self, el: u8) -> Result<(), DequeFull> {
        if self.is_full() {
            Err(DequeFull)
        } else {
            let thing: NatPolynomial<BOUND> =
                NatPolynomial::X + NatPolynomial::try_from(el).unwrap() - NatPolynomial::ONE;

            let mut power = NatPolynomial::ONE;
            while power <= self.0 {
                power.mul_x();
            }
            power.drop_constant_and_divide_x();

            self.0 = self.0 + thing * power;
            Ok(())
        }
    }

    pub fn pop_high(&mut self) -> Result<u8, DequeEmpty> {
        if self.is_empty() {
            Err(DequeEmpty)
        } else {
            // Remove the leading one.
            let mut leading: NatPolynomial<BOUND> = NatPolynomial::ONE;
            while leading < self.0 {
                leading.mul_x();
            }
            leading.drop_constant_and_divide_x();
            self.0 = self.0 - leading;

            // Get the next term.
            leading.drop_constant_and_divide_x();
            let mut out = 0;
            while self.0 >= leading {
                out += 1;
                self.0 = self.0 - leading;
            }

            // Restore the leading zero.
            self.0 = self.0 + leading;

            Ok(out)
        }
    }
}

#[cfg(test)]
mod tests {
    use crate::deque::Deque;

    #[test]
    fn push_pop_low() {
        let mut deque = Deque::<10, 4>::new_empty();

        for (_len, digit) in (1..=4).zip([1, 3, 3, 7].into_iter()) {
            deque.push_low(digit).unwrap();
        }

        for (_len, digit) in (1..5).zip([1, 3, 3, 7].into_iter()).rev() {
            assert_eq!(deque.pop_low().unwrap(), digit);
        }
    }

    #[test]
    fn push_pop_high() {
        let mut deque = Deque::<10, 4>::new_empty();

        for (_len, digit) in (1..=4).zip([1, 3, 3, 7].into_iter()) {
            deque.push_high(digit).unwrap();
        }

        for (_len, digit) in (1..5).zip([1, 3, 3, 7].into_iter()).rev() {
            assert_eq!(deque.pop_high().unwrap(), digit);
        }
    }

    #[test]
    fn push_low_pop_high() {
        let mut deque = Deque::<10, 4>::new_empty();

        for (_len, digit) in (1..=4).zip([1, 3, 3, 7].into_iter()) {
            deque.push_low(digit).unwrap();
        }

        for (_len, digit) in (1..=4).zip([1, 3, 3, 7].into_iter()) {
            assert_eq!(deque.pop_high().unwrap(), digit);
        }
    }

    #[test]
    fn push_high_pop_low() {
        let mut deque = Deque::<10, 4>::new_empty();

        for (_len, digit) in (1..=4).zip([1, 3, 3, 7].into_iter()) {
            deque.push_high(digit).unwrap();
        }

        for (_len, digit) in (1..=4).zip([1, 3, 3, 7].into_iter()) {
            assert_eq!(deque.pop_low().unwrap(), digit);
        }
    }
}
