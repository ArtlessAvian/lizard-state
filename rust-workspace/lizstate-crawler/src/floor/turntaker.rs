use crate::commands::CommandTrait;
use crate::commands::StepMacro;
use crate::creature::Creature;
use crate::floor::Floor;
use crate::floor::creatures::Turn;

#[derive(Debug)]
#[must_use]
pub struct Turntaker<'a> {
    id: u8,
    now: Turn,
    creature: &'a Creature,
    floor: &'a Floor,
}

impl<'a> Turntaker<'a> {
    #[must_use]
    pub fn try_from_floor(floor: &'a Floor) -> Option<Self> {
        let (id, now, creature) = floor.get_creature_list().iter_turn_order().next()?;

        Some(Self {
            id,
            now,
            creature,
            floor,
        })
    }
}

impl Turntaker<'_> {
    #[must_use]
    pub fn get_id(&self) -> u8 {
        self.id
    }

    pub fn get_creature(&self) -> &Creature {
        self.creature
    }

    pub fn get_floor(&self) -> &Floor {
        self.floor
    }

    pub fn get_now(&self) -> &Turn {
        &self.now
    }

    /// Currently an arbitrary command.
    pub fn take_npc_turn(&self) -> Floor {
        let command = StepMacro(crate::spatial::grid::KingStep::East);
        command.do_or_wait(self)
    }

    /// Limits modifications to the turntaker before creating a new Floor.
    /// # Errors
    /// Errors when passed function errors.
    /// # Panics
    /// Turntaker struct is invalid.
    pub fn map_independent<E>(
        &self,
        mapper: impl Fn(&Creature, &Floor) -> Result<Creature, E>,
    ) -> Result<Floor, E> {
        let new_creature = mapper(self.get_creature(), self.get_floor())?;

        let mut new_floor = self.get_floor().clone();
        let mut_creature = new_floor
            .get_creature_list_mut()
            .get_creature_mut_or_insert(self.id, self.creature);
        *mut_creature = new_creature;

        Ok(new_floor)
    }
}

impl PartialEq for Turntaker<'_> {
    fn eq(&self, other: &Self) -> bool {
        self.id == other.id
            && self.now == other.now
            && std::ptr::eq(self.creature, other.creature)
            && std::ptr::eq(self.floor, other.floor)
    }
}
