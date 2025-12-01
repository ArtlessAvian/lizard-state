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
    pub fn outer_path_iter(self) -> impl Iterator<Item = Cardinal> {
        const NORTH: u8 = const { Cardinal::North as u8 + 1 };
        const SOUTH: u8 = const { Cardinal::South as u8 + 1 };
        const EAST: u8 = const { Cardinal::East as u8 + 1 };
        const WEST: u8 = const { Cardinal::West as u8 + 1 };

        self.outer_bytes().into_iter().filter_map(|x| match x {
            0 => None,
            NORTH => Some(Cardinal::North),
            SOUTH => Some(Cardinal::South),
            EAST => Some(Cardinal::East),
            WEST => Some(Cardinal::West),
            _ => None,
        })
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
            (to_div_mod / 8).cast_signed(),
        )
    }

    fn inner_byte(self) -> u8 {
        let [inner, ..] = self.0;
        inner
    }

    fn outer_bytes(self) -> [u8; 7] {
        let [_, outer @ ..] = self.0;
        outer
    }

    fn mut_inner_outer_bytes(&mut self) -> (&mut u8, [&mut u8; 7]) {
        let [inner, outer @ ..] = self.0.each_mut();
        (inner, outer)
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
    fn step(mut self, dir: Cardinal) -> Self {
        let carry = match dir {
            Cardinal::North => self.inner_byte() < 8,
            Cardinal::South => self.inner_byte() >= 8 * 7,
            Cardinal::East => self.inner_byte() % 8 == 7,
            #[expect(clippy::manual_is_multiple_of, reason = "consistency")]
            Cardinal::West => self.inner_byte() % 8 == 0,
        };

        let (inner, outer) = self.mut_inner_outer_bytes();

        // Go seven steps the other way, or go the normal way.
        let c = if carry { -7i8 } else { 1 };
        match dir {
            Cardinal::North => *inner = inner.wrapping_add_signed(-8 * c),
            Cardinal::South => *inner = inner.wrapping_add_signed(8 * c),
            Cardinal::East => *inner = inner.wrapping_add_signed(c),
            Cardinal::West => *inner = inner.wrapping_add_signed(-c),
        }

        // Do the outer step.
        if carry {
            // Lets be boring. One byte per `Option<Cardinal>`.

            let mut previous: Option<&mut u8> = None;
            let mut found = false;
            for current in outer {
                if *current != 0 {
                    previous = Some(current);
                } else {
                    if let Some(top) = previous
                        && *top == (dir.flip() as u8) + 1
                    {
                        *top = 0;
                    } else {
                        *current = (dir as u8) + 1;
                    }
                    found = true;
                    break;
                }
            }

            assert!(found, "Step failed, outer path too full!");
        }

        self
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
        let position = PathAndChunk([
            0,
            Cardinal::West as u8 + 1,
            Cardinal::West as u8 + 1,
            Cardinal::West as u8 + 1,
            Cardinal::West as u8 + 1,
            Cardinal::West as u8 + 1,
            Cardinal::West as u8 + 1,
            Cardinal::West as u8 + 1,
        ]);
        assert_eq!(position.inner_chunk(), (0, 0));
        assert!(
            position
                .outer_path_sized()
                .into_iter()
                .flatten()
                .eq([Cardinal::West; 7])
        );

        let _should_panic = position.step(Cardinal::West);
    }
}
