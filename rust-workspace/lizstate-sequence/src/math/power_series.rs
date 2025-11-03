/// A true equality involving a possibly infinite power series.
///
/// The trait makes no guarantees about how fast these functions are.
/// Comments will refer to P(x) = \sum_{i \in N} (a_i)(x^i)
/// Comments include 0 as a natural number.
pub trait PowerSeriesEquality {
    type Coeff;
    type Domain;
    type Range;

    /// Either calculates or retrieves the nth coefficient of the power series in the equality.
    /// This coefficient may be 0.
    ///
    /// If the coefficient is not known, we must know x and P(x).
    /// We can divide by x and floor. Integers do it for you automatically.
    fn get_coeff(&self, exponent: u8) -> Self::Coeff;

    /// Solves the polynomial or retrieves the x from the equality.
    ///
    /// Usually, you should worry about positive x, unless you are solving or doing great evil.
    fn get_x(&self) -> Self::Domain;

    /// Evaluates the polynomial or retrieves the value from the equality.
    fn get_sum(&self) -> Self::Range;

    fn get_if_sum_is_zero(&self) -> bool;
}

/// Given known equalities, you can do algebra on them.
/// These ones assume x is fixed.
/// Here's some limited stuff. This also serves as the mutable interface.
///
/// P(1/x) =
pub trait PowerSeriesAlgebra: PowerSeriesEquality {
    const X: Self::Domain;

    fn set_coeff(&mut self, exponent: u8, coeff: Self::Coeff);

    /// Shifts all coefficient to a higher order term.
    ///
    ///   x * \sum_n (a_n * x^n)
    /// = \sum_n (x * a_n * x^n)
    /// = \sum_n (a_n * x^{n+1})
    fn multiply_x_and_add(&mut self, coeff: Self::Coeff);

    /// Shifts all coefficient to a lower order term.
    /// The lowest order term is returned.
    ///
    ///   (1/x) * (\sum_n (a_n * x^n))
    /// = (1/x) * (a_0 + (\sum_{n > 0} (a_n * x^n)))
    /// = (a_0/x) + (\sum_{n} (a_{n + 1} * x^n))
    fn divide_x_and_discard(&mut self) -> Self::Coeff;

    fn into_iter_coeffs(self) -> CoeffIterator<Self>
    where
        Self: Sized,
    {
        CoeffIterator(self)
    }
}

#[derive(Debug, Clone, Copy, PartialEq, Eq)]
pub struct CoeffIterator<T: PowerSeriesAlgebra>(T);

impl<T: PowerSeriesAlgebra> Iterator for CoeffIterator<T> {
    type Item = T::Coeff;

    fn next(&mut self) -> Option<Self::Item> {
        Some(self.0.divide_x_and_discard())
    }
}

impl<T: PowerSeriesAlgebra> CoeffIterator<T> {
    pub fn is_infinite_zeroes(&self) -> bool {
        self.0.get_if_sum_is_zero()
    }
}
