use core::borrow::Borrow;
use core::ops::Index;
use std::collections::HashMap;
use std::hash::Hash;

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

    fn insert(&mut self, key: Self::Key, tile: MapTile);
}

pub struct GridMap<Key>(HashMap<Key, MapTile>);

impl<Key: GridLike> MapLike for GridMap<Key> {
    type Key = Key;
    const DEFAULT: MapTile = MapTile::Floor;

    fn insert(&mut self, key: Self::Key, tile: MapTile) {
        if tile == Self::DEFAULT {
            self.0.remove(&key);
        } else {
            let _ = self.0.insert(key, tile);
        }
    }
}

impl<Key, Q> Index<&Q> for GridMap<Key>
where
    Key: GridLike + Borrow<Q>,
    Q: Eq + Hash,
{
    type Output = MapTile;

    fn index(&self, index: &Q) -> &Self::Output {
        self.0.get(index).unwrap_or(&Self::DEFAULT)
    }
}
