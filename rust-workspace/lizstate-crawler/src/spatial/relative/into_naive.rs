use crate::spatial::relative::Cardinal;
use crate::spatial::relative::Vec2i;

#[must_use]
pub struct IntoNaive(Vec2i);
impl IntoNaive {
    pub(crate) fn new(vec: Vec2i) -> Self {
        Self(vec)
    }
}

impl Iterator for IntoNaive {
    type Item = Cardinal;

    fn next(&mut self) -> Option<Self::Item> {
        if self.0.0 > 0 {
            self.0.0 -= 1;
            Some(Cardinal::East)
        } else if self.0.0 < 0 {
            self.0.0 += 1;
            Some(Cardinal::West)
        } else if self.0.1 > 0 {
            self.0.1 -= 1;
            Some(Cardinal::South)
        } else if self.0.1 < 0 {
            self.0.1 += 1;
            Some(Cardinal::North)
        } else {
            None
        }
    }
}

#[cfg(test)]
mod tests {
    use crate::spatial::relative::Cardinal;
    use crate::spatial::relative::Vec2i;

    #[test]
    fn naive() {
        let naive = Vec2i(4, 3).into_naive().collect::<Vec<_>>();
        assert_eq!(
            naive,
            [
                Cardinal::East,
                Cardinal::East,
                Cardinal::East,
                Cardinal::East,
                Cardinal::South,
                Cardinal::South,
                Cardinal::South,
            ]
        );

        let naive = Vec2i(-3, -4).into_naive().collect::<Vec<_>>();
        assert_eq!(
            naive,
            [
                Cardinal::West,
                Cardinal::West,
                Cardinal::West,
                Cardinal::North,
                Cardinal::North,
                Cardinal::North,
                Cardinal::North,
            ]
        );
    }
}
