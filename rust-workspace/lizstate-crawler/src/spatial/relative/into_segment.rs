use core::convert::identity;

use crate::spatial::relative::Cardinal;
use crate::spatial::relative::Vec2i;

fn reflect_x(cardinal: Cardinal) -> Cardinal {
    match cardinal {
        Cardinal::East => Cardinal::West,
        Cardinal::West => Cardinal::East,
        _ => cardinal,
    }
}

fn reflect_y(cardinal: Cardinal) -> Cardinal {
    match cardinal {
        Cardinal::North => Cardinal::South,
        Cardinal::South => Cardinal::North,
        _ => cardinal,
    }
}

fn reflect_both(cardinal: Cardinal) -> Cardinal {
    reflect_x(reflect_y(cardinal))
}

#[allow(clippy::type_complexity, reason = "temporary impl detail")]
const REFLECTIONS: [[fn(Cardinal) -> Cardinal; 2]; 2] =
    [[identity, reflect_y], [reflect_x, reflect_both]];

/// A segment, terminating early if it crosses a corner EXACTLY.
#[derive(Debug)]
#[must_use]
pub struct IntoSegment {
    target: (u32, u32),
    // PRECONDITION 2: A point in the square
    // [current.0 - 0.5, current.0 + 0.5) x [current.1 - 0.5, current.1 + 0.5)
    // is contained in the segment from (0, 0) to self.target.
    current: (u32, u32),
    reflection: fn(Cardinal) -> Cardinal,
}

impl IntoSegment {
    pub(crate) fn new(vec: Vec2i) -> Self {
        Self {
            target: (vec.0.unsigned_abs(), vec.1.unsigned_abs()),
            // PRECONDITION 2: (0, 0) is in both the square and the segment.
            current: (0, 0),
            reflection: REFLECTIONS[usize::from(vec.0 < 0)][usize::from(vec.1 < 0)],
        }
    }

    #[must_use]
    pub fn terminates_early(&self) -> bool {
        let mut target = (self.target.0, self.target.1);
        // Segments should be symmetric.
        // If the segment can be divided in two and contains an exact corner, then both halves contains an exact corner.
        while target.0.is_multiple_of(2) && target.1.is_multiple_of(2) {
            target.0 /= 2;
            target.1 /= 2;
        }

        // There's x EASTs and y SOUTHs.
        // If x + y is even, then we can split the segments into two halves.
        // However, we can't split it like we did above.
        // If y is odd, then we cannot split the SOUTHs evenly between the two.
        (target.0 + target.1).is_multiple_of(2) && !target.1.is_multiple_of(2)
    }

    fn can_go_south(&self) -> bool {
        if self.target.1 == self.current.1 {
            false
        } else {
            self.target.0 * (2 * self.current.1 + 1) < self.target.1 * (2 * self.current.0 + 1)
        }
    }

    fn can_go_east(&self) -> bool {
        if self.target.0 == self.current.0 {
            false
        } else {
            self.target.1 * (2 * self.current.0 + 1) < self.target.0 * (2 * self.current.1 + 1)
        }
    }
}

impl Iterator for IntoSegment {
    type Item = Cardinal;

    fn next(&mut self) -> Option<Self::Item> {
        dbg!(&self);

        if self.can_go_south() {
            self.current.1 += 1;
            return Some((self.reflection)(Cardinal::South));
        }
        if self.can_go_east() {
            self.current.0 += 1;
            return Some((self.reflection)(Cardinal::East));
        }

        // We're at the end!
        None
    }
}

#[cfg(test)]
mod tests {
    use crate::spatial::relative::Cardinal;
    use crate::spatial::relative::Vec2i;

    #[test]
    fn segment() {
        // O-
        //  --
        //   --
        //    -X
        let path = Vec2i(4, 3)
            .into_segment_maybe_terminating()
            .collect::<Vec<_>>();
        assert_eq!(
            path,
            [
                Cardinal::East,
                Cardinal::South,
                Cardinal::East,
                Cardinal::South,
                Cardinal::East,
                Cardinal::South,
                Cardinal::East,
            ]
        );

        let path = Vec2i(-3, -4)
            .into_segment_maybe_terminating()
            .collect::<Vec<_>>();
        assert_eq!(
            path,
            [
                Cardinal::North,
                Cardinal::West,
                Cardinal::North,
                Cardinal::West,
                Cardinal::North,
                Cardinal::West,
                Cardinal::North,
            ]
        );
    }

    #[test]
    fn early_termination() {
        // We can't choose between [EAST EAST SOUTH EAST] or [EAST SOUTH EAST EAST].
        // So we just give up.
        let when_the = Vec2i(3, 1)
            .into_segment_maybe_terminating()
            .collect::<Vec<_>>();
        assert_ne!(when_the.len(), 4, "{when_the:?}");
        assert!(
            Vec2i(3, 1)
                .into_segment_maybe_terminating()
                .terminates_early()
        );

        // This segment can is a superset of the first.
        let when_the = Vec2i(6, 2)
            .into_segment_maybe_terminating()
            .collect::<Vec<_>>();
        assert_ne!(when_the.len(), 8, "{when_the:?}");
        assert!(
            Vec2i(6, 2)
                .into_segment_maybe_terminating()
                .terminates_early()
        );

        // If we want to do that, we must intentionally bias towards X or Y and overshoot.
        let biased = Vec2i(300, 101)
            .into_segment_maybe_terminating()
            .take(8)
            .collect::<Vec<_>>();
        assert_eq!(
            biased,
            [
                Cardinal::East,
                Cardinal::South,
                Cardinal::East,
                Cardinal::East
            ]
            .repeat(2)
        );
        let biased = Vec2i(301, 100)
            .into_segment_maybe_terminating()
            .take(8)
            .collect::<Vec<_>>();
        assert_eq!(
            biased,
            [
                Cardinal::East,
                Cardinal::East,
                Cardinal::South,
                Cardinal::East
            ]
            .repeat(2)
        );
    }

    #[test]
    fn perfectly_diagonal_terminates_instantly() {
        assert!(
            Vec2i(100, 100)
                .into_segment_maybe_terminating()
                .next()
                .is_none()
        );
        assert!(
            Vec2i(-200, 200)
                .into_segment_maybe_terminating()
                .next()
                .is_none()
        );
        assert!(
            Vec2i(300, -300)
                .into_segment_maybe_terminating()
                .next()
                .is_none()
        );
        assert!(
            Vec2i(-400, -400)
                .into_segment_maybe_terminating()
                .next()
                .is_none()
        );

        assert!(
            Vec2i(100, 100)
                .into_segment_maybe_terminating()
                .terminates_early()
        );
        assert!(
            Vec2i(-200, 200)
                .into_segment_maybe_terminating()
                .terminates_early()
        );
        assert!(
            Vec2i(300, -300)
                .into_segment_maybe_terminating()
                .terminates_early()
        );
        assert!(
            Vec2i(-400, -400)
                .into_segment_maybe_terminating()
                .terminates_early()
        );
    }
}
