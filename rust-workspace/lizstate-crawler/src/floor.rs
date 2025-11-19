use crate::creature::Creature;
use crate::floor::creatures::CreatureList;
use crate::floor::creatures::Turn;
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
    pub fn get_creatures(&self) -> impl Iterator<Item = (u8, &Creature)> {
        self.creatures.iter_indices_nonempty()
    }

    pub fn get_creature_mut(&mut self, index: u8) -> Option<&mut Creature> {
        self.creatures.get_creature_mut(index)
    }

    pub fn iter_turn_order(&self) -> impl Iterator<Item = (u8, Turn, &Creature)> {
        self.creatures.iter_turn_order()
    }

    pub fn new_test() -> Self {
        let mut creatures = [const { None }; 2];
        creatures[0] = Some(Creature::new(GridPosition(0, 0), 0, 0x854C_30FF));
        creatures[1] = Some(Creature::new(GridPosition(-2, 0), 0, 0xE38F_D5FF));

        Floor {
            creatures: CreatureList::new_from_iter(creatures.into_iter().flatten()),
        }
    }

    #[must_use]
    pub fn try_into_turntaker(&self) -> Option<Turntaker<'_>> {
        Turntaker::try_from_floor(self)
    }
}
