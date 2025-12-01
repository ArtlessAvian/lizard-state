use core::array::from_fn;

use crate::creature::Creature;
use crate::creature::CreatureState;
use crate::entity::Entity;
use crate::floor::creatures::CreatureList;
use crate::floor::turntaker::Turntaker;
use crate::spatial::grid::GridLike;
use crate::spatial::paths_and_chunks::PathAndChunk;
use crate::spatial::relative::Vec2i;

pub mod turntaker;

mod creatures;

#[derive(Debug, Clone)]
#[must_use]
pub struct Floor {
    creatures: CreatureList,
}

impl Floor {
    pub fn get_creature_list(&self) -> &CreatureList {
        &self.creatures
    }

    pub fn get_creature_list_mut(&mut self) -> &mut CreatureList {
        &mut self.creatures
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

        Floor {
            creatures: creatures.into_iter().collect(),
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
        }

        cursor == other.get_position()
    }
}
