use crate::math::power_series::PowerSeriesAlgebra;
use crate::math::power_series::PowerSeriesEquality;
use crate::math::ring::Ring;

pub mod periodic;
pub mod terminating;

pub trait GeneratingFunction {
    type Field: Ring;
    const X: Self::Field;

    fn get_value(&self) -> Self::Field;

    fn from_value(val: Self::Field) -> Self;

    fn from_iter(from: impl IntoIterator<Item = Self::Field>) -> Self
    where
        Self: Sized,
    {
        let mut sum = Self::Field::ZERO;
        let mut pow = Self::Field::ONE;
        for coeff in from {
            sum = sum + coeff * pow;
            pow = pow * Self::X;
        }
        Self::from_value(sum)
    }
}

impl<T: GeneratingFunction> PowerSeriesEquality for T {
    type Coeff = u8;
    type Domain = T::Field;
    type Range = T::Field;

    fn get_coeff(&self, exponent: u8) -> Self::Coeff {
        let (tail, _) = self
            .get_value()
            .inv_mul_whole_add(Self::X.pow_u8(exponent))
            .unwrap();
        let (_, term) = tail.inv_mul_whole_add(Self::X).unwrap();
        term
    }

    fn get_x(&self) -> Self::Domain {
        Self::X
    }

    fn get_sum(&self) -> Self::Range {
        self.get_value()
    }

    fn get_if_sum_is_zero(&self) -> bool {
        self.get_value() == T::Field::ZERO
    }
}

impl<T: GeneratingFunction> PowerSeriesAlgebra for T {
    const X: Self::Domain = Self::X;

    fn set_coeff(&mut self, exponent: u8, coeff: Self::Coeff) {
        let pow = Self::X.pow_u8(exponent);
        let old = self.get_coeff(exponent);

        let value = self.get_value() - T::Field::from(old) * pow + T::Field::from(coeff) * pow;
        *self = Self::from_value(value)
    }

    fn multiply_x_and_add(&mut self, coeff: Self::Coeff) {
        *self = Self::from_value(self.get_value() * Self::X + T::Field::from(coeff))
    }

    // Return the lowest order term, keep the higher order terms.
    fn divide_x_and_discard(&mut self) -> Self::Coeff {
        let (div, remainder) = self.get_value().inv_mul_whole_add(Self::X).unwrap();

        *self = Self::from_value(div);

        remainder
    }
}
