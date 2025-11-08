#![allow(unused, reason = "WIP")]

use core::array::from_fn;

use crate::math::commutative_ring::CommutativeRing;
use crate::math::polynomial::PolynomialRing;
use crate::math::polynomial::nat::NatPolynomial;

#[derive(Debug, Clone, Copy, PartialEq, Eq)]
pub struct DequeFull;
#[derive(Debug, Clone, Copy, PartialEq, Eq)]
pub struct DequeEmpty;

/// Deque, impossible to misuse, but not inherently useful.
/// Equivalent to `[u8; 7]`
pub type NaiveDeque = Deque<256, 7>;

#[derive(Debug, Clone, Copy)]
pub struct Deque<const BOUND_EXCLUSIVE: u16, const CAPACITY: u8 = 8>(
    NatPolynomial<BOUND_EXCLUSIVE>,
);

impl<const BOUND: u16, const CAP: u8> Deque<BOUND, CAP> {
    const _REASONABLE_BOUND: () = {
        assert!(
            !(BOUND > u8::MAX as u16 + 1),
            "You aren't saving saving much memory with this. Also the deque interface assumes u8s."
        );
        assert!(BOUND != 0, "This is a Deque<!>");
        assert!(BOUND != 1, "This is a Deque<()>, which is just a number.");
    };

    const _REASONABLE_CAP: () = {
        assert!(CAP != 0, "why");
        assert!(CAP != 1, "Use Option<T> instead");
    };

    const _CAPACITY_FITS: () = {
        assert!(
            Self(NatPolynomial::from_raw(u64::MAX)).is_full(),
            "Can't store this many elements!"
        )
    };

    pub const fn new_empty() -> Self {
        Self::_CAPACITY_FITS;

        Deque(NatPolynomial::ONE)
    }

    pub fn is_empty(&self) -> bool {
        self.0 <= NatPolynomial::ONE
    }

    pub const fn len(&self) -> usize {
        self.0
            .get_degree()
            .expect("self.0 is never NatPolynomial::ZERO")
    }

    pub const fn is_full(&self) -> bool {
        self.len() >= (CAP as usize)
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
            let out = self.0.get_constant_coeff();
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
            let mut hack = self.0;
            hack.drop_constant_and_divide_x();
            let hack = hack;

            let mut leading: NatPolynomial<BOUND> = NatPolynomial::X;
            while leading <= hack {
                leading.mul_x();
            }
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

    /// Returns an fixed size array and an index.
    /// A little more wieldy than iter().collect().
    pub fn to_array(self) -> ([u8; 64], usize) {
        let mut iter = self.into_iter_low_to_high();
        let out: [u8; 64] = from_fn(|_| iter.next().unwrap_or(u8::MAX));
        (out, self.len())
    }

    pub fn into_iter_low_to_high(self) -> LowToHighIter<BOUND, CAP> {
        LowToHighIter(self)
    }

    pub fn into_iter_high_to_low(self) -> impl Iterator<Item = u8> {
        self.into_iter_low_to_high().rev()
    }
}

pub struct LowToHighIter<const BOUND: u16, const CAP: u8>(Deque<BOUND, CAP>);

impl<const BOUND: u16, const CAP: u8> IntoIterator for Deque<BOUND, CAP> {
    type Item = u8;
    type IntoIter = LowToHighIter<BOUND, CAP>;

    fn into_iter(self) -> Self::IntoIter {
        LowToHighIter(self)
    }
}

impl<const BOUND: u16, const CAP: u8> Iterator for LowToHighIter<BOUND, CAP> {
    type Item = u8;

    fn next(&mut self) -> Option<Self::Item> {
        self.0.pop_low().ok()
    }
}

impl<const BOUND: u16, const CAP: u8> DoubleEndedIterator for LowToHighIter<BOUND, CAP> {
    fn next_back(&mut self) -> Option<Self::Item> {
        self.0.pop_high().ok()
    }
}

#[cfg(test)]
mod tests {
    use std::vec::Vec;

    use crate::deque::Deque;
    use crate::deque::NaiveDeque;

    #[test]
    fn stack_interface() {
        let mut push_pop_low = NaiveDeque::new_empty();
        [8, 7, 6, 5, 3, 0, 9]
            .into_iter()
            .try_for_each(|e| push_pop_low.push_low(e))
            .unwrap();
        assert_eq!(
            push_pop_low.into_iter_low_to_high().collect::<Vec<_>>(),
            [9, 0, 3, 5, 6, 7, 8]
        );

        let mut push_pop_high = NaiveDeque::new_empty();
        [8, 7, 6, 5, 3, 0, 9]
            .into_iter()
            .try_for_each(|e| push_pop_high.push_high(e))
            .unwrap();
        assert_eq!(
            push_pop_high.into_iter_high_to_low().collect::<Vec<_>>(),
            [9, 0, 3, 5, 6, 7, 8]
        );
    }

    #[test]
    fn queue_interface() {
        let mut push_low = NaiveDeque::new_empty();
        [8, 7, 6, 5, 3, 0, 9]
            .into_iter()
            .try_for_each(|e| push_low.push_low(e))
            .unwrap();
        assert_eq!(
            push_low.into_iter_high_to_low().collect::<Vec<_>>(),
            [8, 7, 6, 5, 3, 0, 9]
        );

        let mut push_high = NaiveDeque::new_empty();
        [8, 7, 6, 5, 3, 0, 9]
            .into_iter()
            .try_for_each(|e| push_high.push_high(e))
            .unwrap();
        assert_eq!(
            push_high.into_iter_low_to_high().collect::<Vec<_>>(),
            [8, 7, 6, 5, 3, 0, 9]
        );
    }

    #[test]
    fn empty() {
        let mut deque = NaiveDeque::new_empty();

        assert!(deque.is_empty());
        deque.pop_low().expect_err("empty cannot pop");
        deque.pop_high().expect_err("empty cannot pop");
    }

    #[test]
    fn full() {
        const CAPACITY: u8 = 14;

        let mut largest_full = {
            let mut deque = Deque::<10, CAPACITY>::new_empty();
            for i in 0..CAPACITY {
                assert_eq!(deque.len(), i as usize);
                deque.push_low(9).unwrap();
            }
            assert_eq!(deque.len(), CAPACITY as usize);
            assert!(deque.is_full());
            deque
        };

        largest_full.push_low(0).expect_err("full cannot push");
        largest_full.push_high(0).expect_err("full cannot push");

        let mut smallest_full = {
            let mut deque = Deque::<10, CAPACITY>::new_empty();
            for i in 0..CAPACITY {
                assert_eq!(deque.len(), i as usize);
                deque.push_low(0).unwrap();
            }
            assert_eq!(deque.len(), CAPACITY as usize);
            assert!(deque.is_full());
            deque
        };

        smallest_full.push_low(0).expect_err("full cannot push");
        smallest_full.push_high(0).expect_err("full cannot push");
    }

    #[test]
    fn iter() {
        let mut empty = NaiveDeque::new_empty();
        assert_eq!(empty.into_iter().count(), 0);

        let mut jennys = Deque::<10>::new_empty();
        jennys.push_high(8).unwrap();
        jennys.push_high(7).unwrap();
        jennys.push_high(6).unwrap();
        jennys.push_high(5).unwrap();
        jennys.push_high(3).unwrap();
        jennys.push_high(0).unwrap();
        jennys.push_high(9).unwrap();
        assert_eq!(
            jennys.into_iter().collect::<Vec<_>>(),
            ([8, 7, 6, 5, 3, 0, 9])
        );

        let mut awoo = Deque::<100>::new_empty();
        awoo.push_low(5).unwrap();
        awoo.push_low(6).unwrap();
        awoo.push_low(7).unwrap();
        awoo.push_low(0).unwrap();
        awoo.push_low(9).unwrap();
        assert_eq!(
            awoo.into_iter().rev().collect::<Vec<_>>(),
            ([5, 6, 7, 0, 9])
        );
    }

    #[test]
    fn to_array() {
        let mut empty = NaiveDeque::new_empty();
        assert_eq!(empty.to_array(), ([255; 64], 0));

        let mut jennys = Deque::<10>::new_empty();
        jennys.push_high(8).unwrap();
        jennys.push_high(7).unwrap();
        jennys.push_high(6).unwrap();
        jennys.push_high(5).unwrap();
        jennys.push_high(3).unwrap();
        jennys.push_high(0).unwrap();
        jennys.push_high(9).unwrap();
        let mut expected = [255u8; 64];
        expected[0] = 8;
        expected[1] = 7;
        expected[2] = 6;
        expected[3] = 5;
        expected[4] = 3;
        expected[5] = 0;
        expected[6] = 9;
        assert_eq!(jennys.to_array(), (expected, 7));

        let mut awoo = Deque::<100>::new_empty();
        awoo.push_low(5).unwrap();
        awoo.push_low(6).unwrap();
        awoo.push_low(7).unwrap();
        awoo.push_low(0).unwrap();
        awoo.push_low(9).unwrap();
        let mut expected = [255u8; 64];
        expected[0] = 9;
        expected[1] = 0;
        expected[2] = 7;
        expected[3] = 6;
        expected[4] = 5;
        assert_eq!(awoo.to_array(), (expected, 5));
    }
}
