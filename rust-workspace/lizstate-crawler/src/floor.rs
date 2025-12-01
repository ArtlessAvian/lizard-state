use core::array::from_fn;
use std::collections::HashMap;

use crate::creature::Creature;
use crate::creature::CreatureState;
use crate::entity::Entity;
use crate::floor::creatures::CreatureList;
use crate::floor::turntaker::Turntaker;
use crate::spatial::grid::GridLike;
use crate::spatial::map::FunctionMap;
use crate::spatial::map::MapLike;
use crate::spatial::map::MapTile;
use crate::spatial::paths_and_chunks::PathAndChunk;
use crate::spatial::relative::Vec2i;

pub mod turntaker;

mod creatures;

#[derive(Debug, Clone)]
#[must_use]
pub struct Floor {
    creatures: CreatureList,
    map: FunctionMap<PathAndChunk>,
}

impl Floor {
    pub fn get_creature_list(&self) -> &CreatureList {
        &self.creatures
    }

    pub fn get_creature_list_mut(&mut self) -> &mut CreatureList {
        &mut self.creatures
    }

    #[must_use]
    pub fn get_map(&self) -> impl MapLike<Key = PathAndChunk> {
        self.map
    }

    pub fn new_test() -> Self {
        let mut creatures: [Creature; 4] = from_fn(|_| Creature::new_garbage());

        creatures[0] = Creature::new(
            PathAndChunk::path_from_origin(Vec2i(0, 0).into_naive()),
            0,
            0x854C_30FF,
            0,
        );
        creatures[1] = Creature::new(
            PathAndChunk::path_from_origin(Vec2i(-2, 0).into_naive()),
            0,
            0xE38F_D5FF,
            0,
        );

        creatures[2] = Creature::new(
            PathAndChunk::path_from_origin(Vec2i(5, 3).into_naive()),
            0,
            0xDAD4_5EFF,
            1,
        );
        *creatures[2].get_state_mut() = CreatureState::Committed { round: 3 };

        creatures[3] = Creature::new_garbage();
        creatures[3].set_position(PathAndChunk::path_from_origin(Vec2i(0, -3).into_naive()));

        let function = |x: PathAndChunk| {
            let flatten = x.flatten();
            let oval = flatten.0 * flatten.0 / 4 + flatten.1 * flatten.1;
            if 0 < oval && oval < 20 {
                MapTile::Floor
            } else {
                MapTile::Wall
            }
        };

        // let function = |x: PathAndChunk| {
        //     if x.flatten().0.rem_euclid(8) == 7 && x.flatten().1.rem_euclid(8) == 7 {
        //         MapTile::Wall
        //     } else {
        //         MapTile::Floor
        //     }
        // };

        Floor {
            creatures: creatures.into_iter().collect(),
            map: FunctionMap(function),
        }
    }

    #[must_use]
    pub fn try_into_turntaker(&self) -> Option<Turntaker<'_>> {
        Turntaker::try_from_floor(self)
    }

    #[must_use]
    pub fn entity_can_see_other(&self, who: u8, other: u8) -> bool {
        let Some(who) = self.creatures.get(who) else {
            return false;
        };
        let Some(other) = self.creatures.get(other) else {
            return false;
        };

        let mut vec = Vec2i(
            other.get_flat_position().0 - who.get_flat_position().0,
            other.get_flat_position().1 - who.get_flat_position().1,
        );

        // overshoot and bias.
        vec.0 *= 2;
        vec.1 *= 2;
        vec.0 += vec.0.signum();

        let mut cursor = who.get_position();
        for step in vec.into_segment_maybe_terminating() {
            if cursor == other.get_position() {
                return true;
            }
            cursor = cursor.step(step);
            if !self.get_map().get(cursor).can_see_through() {
                return false;
            }
        }

        cursor == other.get_position()
    }

    #[must_use]
    pub fn get_entity_vision(&self, who: u8) -> HashMap<(i32, i32), MapTile> {
        let Some(who) = self.creatures.get(who) else {
            return HashMap::new();
        };

        let mut out = HashMap::new();
        let cross = (-20..=20)
            .map(|x| (x, -20))
            .chain((-20..=20).map(|x| (x, 20)))
            .chain((-20..=20).map(|y| (-20, y)))
            .chain((-20..=20).map(|y| (20, y)));

        for (x, y) in cross {
            let path = Vec2i(x, y).into_segment_maybe_terminating().take(20);
            let mut cursor = who.get_position();
            for dir in path {
                cursor = cursor.step(dir);
                out.insert(cursor.flatten(), self.get_map().get(cursor));
                if self.get_map().get(cursor) == MapTile::Wall {
                    break;
                }
            }
        }
        out
    }
}
