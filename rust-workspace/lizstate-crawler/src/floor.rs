use core::array::from_fn;

use crate::creature::Creature;
use crate::creature::CreatureState;
use crate::floor::creatures::CreatureList;
use crate::floor::turntaker::Turntaker;
use crate::spatial::grid::GridPosition;

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

        creatures[0] = Creature::new(GridPosition(0, 0), 0, 0x854C_30FF, 0);
        creatures[1] = Creature::new(GridPosition(-2, 0), 0, 0xE38F_D5FF, 0);

        creatures[2] = Creature::new(GridPosition(5, 3), 0, 0xDAD4_5EFF, 1);
        *creatures[2].get_state_mut() = CreatureState::Committed { round: 3 };

        creatures[3] = Creature::new_garbage();
        creatures[3].set_position(GridPosition(0, -3));

        Floor {
            creatures: creatures.into_iter().collect(),
        }
    }

    #[must_use]
    pub fn try_into_turntaker(&self) -> Option<Turntaker<'_>> {
        Turntaker::try_from_floor(self)
    }
}
