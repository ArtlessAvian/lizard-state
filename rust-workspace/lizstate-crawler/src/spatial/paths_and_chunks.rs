use core::array::from_fn;
use std::hash::Hash;

use crate::spatial::grid::GridLike;
use crate::spatial::relative::Cardinal;

/// 8 bytes! 8x8 chunks, connected like a free group.
///
/// The 8 byte size is LITERALLY part of the public api.
#[derive(Debug, Clone, Copy)]
#[must_use]
pub struct PathAndChunk([u8; 8]);

impl PathAndChunk {
    /// # Panics
    /// Panics if the outer path cannot fit in this struct.
    pub fn from_inner_outer(inner_byte: u8, outer: u64) -> Self {
        let Some(shifted) = outer.checked_mul(1 << 8) else {
            panic!("Outer path too full!")
        };
        PathAndChunk(u64::to_ne_bytes(shifted + u64::from(inner_byte)))
    }

    pub fn outer_path_iter(self) -> impl Iterator<Item = Cardinal> {
        const NORTH: u64 = const { Cardinal::North as u64 + 1 };
        const SOUTH: u64 = const { Cardinal::South as u64 + 1 };
        const EAST: u64 = const { Cardinal::East as u64 + 1 };
        const WEST: u64 = const { Cardinal::West as u64 + 1 };

        let mut outer_number = self.outer_number();
        let mut reverse: [Option<Cardinal>; 27] = [None; 27];
        for mut_opt in &mut reverse {
            *mut_opt = match outer_number % 5 {
                0 => None,
                NORTH => Some(Cardinal::North),
                SOUTH => Some(Cardinal::South),
                EAST => Some(Cardinal::East),
                WEST => Some(Cardinal::West),
                _ => unreachable!(),
            };
            outer_number /= 5;
        }

        let unreversed = {
            reverse.reverse();
            reverse
        };
        unreversed.into_iter().flatten()
    }

    /// A sized representation of the outer path.
    ///
    /// This is used for the Eq and Hash implementations.
    /// With 52 bits, the theoretical maximum capacity that can fit is 27 Cardinals.
    fn outer_path_sized(self) -> [Option<Cardinal>; 27]
    where
        [Option<Cardinal>; 27]: Sized,
    {
        let mut iter = self.outer_path_iter();
        from_fn(|_| iter.next())
    }

    #[must_use]
    pub fn inner_chunk(self) -> (i8, i8) {
        let to_div_mod = self.inner_byte();
        (
            (to_div_mod % 8).cast_signed(),
            ((to_div_mod / 8) % 8).cast_signed(),
        )
    }

    fn inner_byte(self) -> u8 {
        let [inner, ..] = self.0;
        inner
    }

    fn outer_number(self) -> u64 {
        u64::from_le_bytes(self.0) >> 8
    }
}

impl PartialEq for PathAndChunk {
    fn eq(&self, other: &Self) -> bool {
        self.inner_chunk() == other.inner_chunk()
            && self.outer_path_sized() == other.outer_path_sized()
    }
}

impl Eq for PathAndChunk {}

impl Hash for PathAndChunk {
    fn hash<H: std::hash::Hasher>(&self, state: &mut H) {
        self.inner_chunk().hash(state);
        self.outer_path_sized().hash(state);
    }
}

impl GridLike for PathAndChunk {
    fn flatten(self) -> (i32, i32) {
        let inner = self.inner_chunk();
        let mut outer = (0, 0);
        for step in self.outer_path_iter() {
            match step {
                Cardinal::North => outer.1 -= 1,
                Cardinal::South => outer.1 += 1,
                Cardinal::East => outer.0 += 1,
                Cardinal::West => outer.0 -= 1,
            }
        }

        (
            i32::from(inner.0) + outer.0 * 8,
            i32::from(inner.1) + outer.1 * 8,
        )
    }

    fn origin() -> Self {
        Self([0; 8])
    }

    /// # Panics
    /// Panics if the outer path is too long.
    fn step(self, dir: Cardinal) -> Self {
        let mut inner = self.inner_byte();

        let carry = match dir {
            Cardinal::North => inner < 8,
            Cardinal::South => inner >= 8 * 7,
            Cardinal::East => inner % 8 == 7,
            #[expect(clippy::manual_is_multiple_of, reason = "consistency")]
            Cardinal::West => inner % 8 == 0,
        };

        // Go seven steps the other way, or go the normal way.
        let c = if carry { -7i8 } else { 1 };
        match dir {
            Cardinal::North => inner = inner.wrapping_add_signed(-8 * c),
            Cardinal::South => inner = inner.wrapping_add_signed(8 * c),
            Cardinal::East => inner = inner.wrapping_add_signed(c),
            Cardinal::West => inner = inner.wrapping_add_signed(-c),
        }

        let mut outer = self.outer_number();

        // Do the outer step.
        if carry {
            if outer % 5 == dir.flip() as u64 + 1 {
                outer /= 5;
            } else {
                outer *= 5;
                outer += dir as u64 + 1;
            }
        }

        Self::from_inner_outer(inner, outer)
    }
}

#[cfg(test)]
mod tests {
    use crate::spatial::grid::GridLike;
    use crate::spatial::paths_and_chunks::PathAndChunk;
    use crate::spatial::relative::Cardinal;

    #[test]
    fn inverses() {
        let position = PathAndChunk::origin();
        assert_eq!(position.step(Cardinal::East).step(Cardinal::West), position);
        assert_eq!(position.step(Cardinal::West).step(Cardinal::East), position);
        assert_eq!(
            position.step(Cardinal::North).step(Cardinal::South),
            position
        );
        assert_eq!(
            position.step(Cardinal::South).step(Cardinal::North),
            position
        );

        let position = PathAndChunk([0, 1, 2, 3, 4, 1, 0, 0]);
        assert_eq!(position.step(Cardinal::East).step(Cardinal::West), position);
        assert_eq!(position.step(Cardinal::West).step(Cardinal::East), position);
        assert_eq!(
            position.step(Cardinal::North).step(Cardinal::South),
            position
        );
        assert_eq!(
            position.step(Cardinal::South).step(Cardinal::North),
            position
        );
    }

    #[test]
    #[should_panic(expected = "full")]
    fn overflows() {
        let position = PathAndChunk([0, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF]);
        assert_eq!(position.inner_chunk(), (0, 0));
        assert_eq!(
            position.outer_path_sized().into_iter().flatten().count(),
            19
        );

        let _should_panic = position.step(Cardinal::West);
    }
}
