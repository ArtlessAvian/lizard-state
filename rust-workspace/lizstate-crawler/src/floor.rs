use core::ops::DerefMut;

use crate::creature::Creature;
use crate::floor::creatures::CreatureList;
use crate::spatial::grid::GridPosition;

#[derive(Debug)]
#[must_use]
pub struct Turntaker<'a>(u8, &'a Creature, &'a Floor);
mod impl_turntaker;

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

    pub fn get_creature_mut(&mut self, index: u8) -> impl DerefMut<Target = Creature> {
        self.creatures.get_creature_mut(index)
    }

    pub fn new_test() -> Self {
        let mut creatures = [const { None }; 2];
        creatures[0] = Some(Creature::new(GridPosition(0, 0)));
        creatures[1] = Some(Creature::new(GridPosition(-2, 0)));

        Floor {
            creatures: CreatureList::new_from_iter(creatures.into_iter().flatten()),
        }
    }

    /// # Panics
    /// Temporary.
    pub fn get_turntaker(&self) -> Turntaker<'_> {
        let (a, b) = self.get_creatures().next().unwrap();
        Turntaker(a, b, self)
    }
}
