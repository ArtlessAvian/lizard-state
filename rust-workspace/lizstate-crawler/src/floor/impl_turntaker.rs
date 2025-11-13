use crate::creature::Creature;
use crate::floor::Floor;
use crate::floor::Turntaker;

impl Turntaker<'_> {
    pub fn get_creature(&self) -> &Creature {
        self.1
    }

    pub fn get_floor(&self) -> &Floor {
        self.2
    }

    /// Limits modifications to the turntaker before creating a new Floor.
    /// # Errors
    /// Errors when passed function errors.
    pub fn map_independent<E>(
        &self,
        mapper: impl Fn(&Creature, &Floor) -> Result<Creature, E>,
    ) -> Result<Floor, E> {
        let new_creature = mapper(self.get_creature(), self.get_floor())?;

        let mut new_floor = self.get_floor().clone();
        let mut mut_creature = new_floor.get_creature_mut(self.0);
        *mut_creature = new_creature;
        drop(mut_creature);

        Ok(new_floor)
    }
}

impl PartialEq for Turntaker<'_> {
    fn eq(&self, other: &Self) -> bool {
        self.0 == other.0 && std::ptr::eq(self.1, other.1) && std::ptr::eq(self.2, other.2)
    }
}
