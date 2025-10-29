use std::collections::HashMap;
use std::hash::Hash;

use crate::spatial::chunks::PositionInsideChunk;
use crate::spatial::grid::GridLike;

#[non_exhaustive]
#[derive(Default, Clone, Copy, PartialEq, Eq)]
pub enum MapTile {
    #[default]
    Wall,
    Floor,
}

impl MapTile {
    #[must_use]
    pub fn can_step_on(self) -> bool {
        match self {
            MapTile::Wall => false,
            MapTile::Floor => true,
        }
    }

    #[must_use]
    pub fn can_see_through(self) -> bool {
        match self {
            MapTile::Wall => false,
            MapTile::Floor => true,
        }
    }
}

pub struct Map<ChunkType> {
    chunks: HashMap<ChunkType, [MapTile; 64]>,
}

impl<ChunkType: GridLike + Eq + Hash> Map<ChunkType> {
    pub fn get_tile(&self, pos: PositionInsideChunk<ChunkType>) -> MapTile {
        let value = self.chunks.get(&pos.chunk);
        value.map(|array| array[pos.index()]).unwrap_or_default()
    }

    pub fn set_tile(&mut self, pos: PositionInsideChunk<ChunkType>, value: MapTile) {
        let array = self
            .chunks
            .entry(pos.chunk)
            .or_insert_with(|| [const { MapTile::Wall }; 64]);

        array[pos.index()] = value;
    }
}
