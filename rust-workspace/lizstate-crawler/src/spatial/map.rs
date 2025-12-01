use std::collections::HashMap;

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

pub trait MapLike {
    type Key: GridLike;
    const DEFAULT: MapTile;

    fn get(&self, key: Self::Key) -> MapTile;
}

#[derive(Debug, Clone, Copy)]
pub struct FunctionMap<Key>(pub fn(Key) -> MapTile);

impl<Key: GridLike> MapLike for FunctionMap<Key> {
    type Key = Key;
    const DEFAULT: MapTile = MapTile::Wall;

    fn get(&self, key: Self::Key) -> MapTile {
        self.0(key)
    }
}

pub trait MutMapLike: MapLike {
    fn insert(&mut self, key: Self::Key, tile: MapTile);
}

pub struct GridMap<Key>(HashMap<Key, MapTile>);

impl<Key: GridLike> MapLike for GridMap<Key> {
    type Key = Key;
    const DEFAULT: MapTile = MapTile::Floor;

    fn get(&self, key: Self::Key) -> MapTile {
        self.0.get(&key).copied().unwrap_or(Self::DEFAULT)
    }
}

impl<Key: GridLike> MutMapLike for GridMap<Key> {
    fn insert(&mut self, key: Self::Key, tile: MapTile) {
        if tile == Self::DEFAULT {
            self.0.remove(&key);
        } else {
            let _ = self.0.insert(key, tile);
        }
    }
}
