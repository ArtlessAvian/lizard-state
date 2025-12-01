use crate::spatial::relative::into_naive::IntoNaive;
use crate::spatial::relative::into_ray::IntoRay;
use crate::spatial::relative::into_segment::IntoSegment;

pub mod into_naive;
pub mod into_ray;
pub mod into_segment;

/// Generating set of `Gridlike` implementors.
///
/// Also the generating set of `Pathlike`s.
/// `Gridlikes` behave like absolute positions.
/// `Pathlikes` behave like relative positions.
#[derive(Debug, Clone, Copy, PartialEq, Eq, Hash)]
pub enum Cardinal {
    North,
    South,
    East,
    West,
}

/// Like a King in chess.
///
/// IRONICALLY. This behaves more like a horsey which moves in an L, except without a long end.
#[derive(Debug, Clone, Copy, PartialEq, Eq, Hash)]
pub enum KingStep {
    North,
    South,
    East,
    West,
    NorthEast,
    NorthWest,
    SouthEast,
    SouthWest,
}

impl KingStep {
    #[must_use]
    pub fn decompose(self) -> (Option<Cardinal>, Option<Cardinal>) {
        match self {
            KingStep::North => (Some(Cardinal::North), None),
            KingStep::South => (Some(Cardinal::South), None),
            KingStep::East => (None, Some(Cardinal::East)),
            KingStep::West => (None, Some(Cardinal::West)),
            KingStep::NorthEast => (Some(Cardinal::North), Some(Cardinal::East)),
            KingStep::NorthWest => (Some(Cardinal::North), Some(Cardinal::West)),
            KingStep::SouthEast => (Some(Cardinal::South), Some(Cardinal::East)),
            KingStep::SouthWest => (Some(Cardinal::South), Some(Cardinal::West)),
        }
    }
}

#[derive(Debug, Clone, Copy, PartialEq, Eq)]
#[must_use]
pub struct Vec2i(pub i32, pub i32);

impl Vec2i {
    pub fn from_cardinal(cardinal: Cardinal) -> Self {
        match cardinal {
            Cardinal::North => Self(0, -1),
            Cardinal::South => Self(0, 1),
            Cardinal::East => Self(1, 0),
            Cardinal::West => Self(-1, 0),
        }
    }

    pub fn from_path(iter: impl IntoIterator<Item = Cardinal>) -> Self {
        let mut x = 0;
        let mut y = 0;
        for dir in iter {
            match dir {
                Cardinal::North => {
                    y -= 1;
                }
                Cardinal::South => {
                    y += 1;
                }
                Cardinal::East => {
                    x += 1;
                }
                Cardinal::West => {
                    x -= 1;
                }
            }
        }
        Self(x, y)
    }

    pub fn into_naive(self) -> IntoNaive {
        IntoNaive::new(self)
    }

    pub fn into_segment_maybe_terminating(self) -> IntoSegment {
        IntoSegment::new(self)
    }

    pub fn into_ray_maybe_terminating(self) -> IntoRay {
        IntoRay::new(self)
    }

    #[must_use]
    pub fn try_into_segment(self) -> Option<IntoSegment> {
        let out = IntoSegment::new(self);
        out.terminates_early().then_some(out)
    }

    #[must_use]
    pub fn try_into_ray(self) -> Option<IntoRay> {
        let out = IntoRay::new(self);
        out.terminates_early().then_some(out)
    }
}

#[cfg(test)]
mod tests {
    use crate::spatial::relative::Vec2i;

    #[test]
    fn to_and_from() {
        let vecs = {
            let coprime_pairs = [(2, 3), (-4, 5), (6, -7), (-8, -9)];
            let swaps = coprime_pairs.map(|(a, b)| (b, a));
            coprime_pairs
                .into_iter()
                .chain(swaps)
                .map(|(x, y)| Vec2i(x, y))
                .collect::<Vec<_>>()
        };

        for vec in vecs.clone() {
            assert_eq!(Vec2i::from_path(vec.into_naive()), vec, "{vec:?}");
        }

        for vec in vecs.clone() {
            assert_eq!(
                Vec2i::from_path(vec.into_segment_maybe_terminating()),
                vec,
                "{vec:?}"
            );
        }
    }
}
