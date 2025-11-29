use crate::spatial::grid::Cardinal;
use crate::spatial::grid::GridLike;
use crate::spatial::grid::GridPosition;

// TODO: Rename me!
#[derive(Debug, Clone, Copy, PartialEq, Eq, Hash)]
pub struct PositionInsideChunk<ChunkType: GridLike = GridPosition> {
    pub chunk: ChunkType,

    // Imagine splitting the i8 into two nibbles.
    // Each nibble can be treated as a u3 and a carry bit.
    // 00 01 02 03 04 05 06 07 ..
    // 10 11 12 13 14 15 16 17 ..
    // 20 21 22 23 24 25 26 27 ..
    // 30 31 32 33 34 35 36 37 ..
    // 40 41 42 43 44 45 46 47 ..
    // 50 51 52 53 54 55 56 57 ..
    // 60 61 62 63 64 65 66 67 ..
    // 70 71 72 73 74 75 76 77 ..
    // .. .. .. .. .. .. .. .. ..
    magic: i8,
}

impl<ChunkType: GridLike> PositionInsideChunk<ChunkType> {
    const CENTER: i8 = 0x44;

    /// # Panics
    /// `inner` argument is out of bounds.
    #[must_use]
    pub fn new(chunk: ChunkType, inner: (i8, i8)) -> Self {
        assert!(-4 <= inner.0);
        assert!(inner.0 < 4);
        assert!(-4 <= inner.1);
        assert!(inner.1 < 4);

        PositionInsideChunk {
            chunk,
            magic: Self::CENTER + (inner.1 << 4) + inner.0,
        }
    }

    fn cardinal_magic(dir: Cardinal) -> i8 {
        match dir {
            Cardinal::North => -1 << 4,
            Cardinal::South => 1 << 4,
            Cardinal::East => 1,
            Cardinal::West => -1,
        }
    }

    fn inner(self) -> (i8, i8) {
        (
            self.magic.rem_euclid(8) - 4,
            self.magic.div_euclid(16).rem_euclid(8) - 4,
        )
    }

    pub fn index(self) -> usize {
        let unsigned = usize::from(self.magic.cast_unsigned());
        (unsigned & 0x70 >> 1) | (unsigned & 0x7)
    }
}

impl<ChunkType: GridLike> GridLike for PositionInsideChunk<ChunkType> {
    fn flatten(self) -> (i32, i32) {
        (
            (self.chunk.flatten().0 * 8) + i32::from(self.inner().0),
            (self.chunk.flatten().1 * 8) + i32::from(self.inner().1),
        )
    }

    fn origin() -> Self {
        PositionInsideChunk {
            chunk: ChunkType::origin(),
            magic: Self::CENTER,
        }
    }

    fn step(mut self, dir: Cardinal) -> Self {
        self.magic = self.magic.wrapping_add(Self::cardinal_magic(dir));

        if self.magic & 0x88u8.cast_signed() != 0 {
            let undo = Self::cardinal_magic(dir).wrapping_mul(-8);
            self.magic = self.magic.wrapping_add(undo);

            self.chunk = self.chunk.step(dir);
        }

        self
    }
}

#[cfg(test)]
mod tests {
    use crate::spatial::chunks::PositionInsideChunk;
    use crate::spatial::grid::Cardinal;
    use crate::spatial::grid::GridLike;
    use crate::spatial::grid::GridPosition;

    #[test]
    fn position_inside_chunk_inner() {
        for x in -4..4 {
            for y in -4..4 {
                let pos = PositionInsideChunk::new(GridPosition(0, 0), (x, y));
                assert_eq!(pos.inner(), (x, y));
            }
        }
    }

    #[test]
    fn position_inside_chunk_step() {
        for x in -4..4 {
            for y in -4..4 {
                let start = PositionInsideChunk::new(GridPosition(0, 0), (x, y));

                let north_south = start.step(Cardinal::North).step(Cardinal::South);
                assert_eq!(north_south, start);

                let south_north = start.step(Cardinal::South).step(Cardinal::North);
                assert_eq!(south_north, start);

                let east_west = start.step(Cardinal::East).step(Cardinal::West);
                assert_eq!(east_west, start);

                let west_east = start.step(Cardinal::West).step(Cardinal::East);
                assert_eq!(west_east, start);
            }
        }
    }
}
