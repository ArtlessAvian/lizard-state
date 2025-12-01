use crate::spatial::relative::Cardinal;
use crate::spatial::relative::Vec2i;

/// An infinite ray, terminating if it crosses a corner EXACTLY.
#[must_use]
pub struct IntoRay {
    target: (u32, u32),
    // PRECONDITION 2: A point in the square
    // [current.0 - 0.5, current.0 + 0.5) x [current.1 - 0.5, current.1 + 0.5)
    // is contained in the ray from (0, 0) to self.target.
    current: (u32, u32),
    rotation: fn(Cardinal) -> Cardinal,
}

impl IntoRay {
    pub(crate) fn new(mut vec: Vec2i) -> Self {
        let mut rotations = 0;
        while vec.0 < 0 || vec.1 < 0 {
            vec = vec.rotate_clockwise();
            rotations += 1;
        }

        Self {
            target: (vec.0.unsigned_abs(), vec.1.unsigned_abs()),
            // PRECONDITION 2: (0, 0) is in both the square and the ray.
            current: (0, 0),
            rotation: Cardinal::ROTATIONS[(4 - rotations) % 4],
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
        self.target.0 * (2 * self.current.1 + 1) < self.target.1 * (2 * self.current.0 + 1)
    }

    fn can_go_east(&self) -> bool {
        self.target.1 * (2 * self.current.0 + 1) < self.target.0 * (2 * self.current.1 + 1)
    }
}

impl Iterator for IntoRay {
    type Item = Cardinal;

    fn next(&mut self) -> Option<Self::Item> {
        if self.current == self.target {
            self.current = (0, 0);
        }

        if self.can_go_south() {
            self.current.1 += 1;
            return Some((self.rotation)(Cardinal::South));
        }
        if self.can_go_east() {
            self.current.0 += 1;
            return Some((self.rotation)(Cardinal::East));
        }

        // We can't go diagonally.
        None
    }
}

#[cfg(test)]
mod tests {
    use crate::spatial::relative::Cardinal;
    use crate::spatial::relative::Vec2i;

    #[test]
    fn ray() {
        // O-
        //  --
        //   --
        //    -X-
        //      --
        //       --
        //        -X
        let path = Vec2i(4, 3)
            .into_ray_maybe_terminating()
            .take(100)
            .collect::<Vec<_>>();
        let expected = [
            Cardinal::East,
            Cardinal::South,
            Cardinal::East,
            Cardinal::South,
            Cardinal::East,
            Cardinal::South,
            Cardinal::East,
        ];
        for (x, e) in path.iter().copied().zip(expected.iter().copied().cycle()) {
            assert_eq!(x, e);
        }

        let path = Vec2i(-3, -4)
            .into_ray_maybe_terminating()
            .take(100)
            .collect::<Vec<_>>();
        let expected = [
            Cardinal::North,
            Cardinal::West,
            Cardinal::North,
            Cardinal::West,
            Cardinal::North,
            Cardinal::West,
            Cardinal::North,
        ];
        for (x, e) in path.iter().copied().zip(expected.iter().copied().cycle()) {
            assert_eq!(x, e);
        }
    }

    #[test]
    fn early_termination() {
        // We can't choose between [EAST EAST SOUTH EAST] or [EAST SOUTH EAST EAST].
        // So we just give up.
        let when_the = Vec2i(3, 1).into_ray_maybe_terminating().collect::<Vec<_>>();
        assert_ne!(when_the.len(), 4, "{when_the:?}");
        assert!(Vec2i(3, 1).into_ray_maybe_terminating().terminates_early());
        assert!(Vec2i(6, 2).into_ray_maybe_terminating().terminates_early());
        assert!(Vec2i(12, 4).into_ray_maybe_terminating().terminates_early());

        // If we want to do that, we must intentionally bias towards X or Y and overshoot.
        let biased = Vec2i(300, 101)
            .into_ray_maybe_terminating()
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
    }

    #[test]
    fn perfectly_diagonal_terminates_instantly() {
        assert!(
            Vec2i(100, 100)
                .into_ray_maybe_terminating()
                .next()
                .is_none()
        );
        assert!(
            Vec2i(-200, 200)
                .into_ray_maybe_terminating()
                .next()
                .is_none()
        );
        assert!(
            Vec2i(300, -300)
                .into_ray_maybe_terminating()
                .next()
                .is_none()
        );
        assert!(
            Vec2i(-400, -400)
                .into_ray_maybe_terminating()
                .next()
                .is_none()
        );
    }
}
