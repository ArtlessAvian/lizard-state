use std::rc::Rc;

use crate::creature::Creature;
use crate::spatial::grid::GridPosition;

pub struct Floor {
    creatures: Vec<Rc<Creature>>,
}

impl Floor {
    pub fn get_creatures(&self) -> impl Iterator<Item = &Creature> {
        self.creatures.iter().map(Rc::as_ref)
    }

    #[must_use]
    pub fn new_test() -> Self {
        Floor {
            creatures: vec![
                Rc::new(Creature::new(GridPosition(0, 0))),
                Rc::new(Creature::new(GridPosition(-2, 0))),
            ],
        }
    }
}
