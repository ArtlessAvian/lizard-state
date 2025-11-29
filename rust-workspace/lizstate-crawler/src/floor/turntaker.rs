use crate::commands::CommandError;
use crate::commands::CommandTrait;
use crate::commands::WaitCommand;
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

    #[must_use]
    pub fn take_turn_if_not_player(&self, player_id: u8) -> Option<Floor> {
        self.take_turn_if(|turntaker| turntaker.get_id() != player_id)
    }

    #[must_use]
    pub fn take_turn_if(&self, predicate: impl Fn(&Self) -> bool) -> Option<Floor> {
        if let Some(forced) = self.creature.get_state().forced_command() {
            return Some(forced.do_or_wait(self));
        }

        if predicate(self) {
            Some(self.take_npc_turn())
        } else {
            None
        }
    }

    /// Currently an arbitrary command.
    fn take_npc_turn(&self) -> Floor {
        let command = WaitCommand;
        command.do_or_wait(self)
    }

    /// Modifies the creature in a clone of the `Floor`.
    ///
    /// No other creature on the `Floor` is modified. However, you may modify them after returning.
    ///
    /// # Panics
    /// The `Turntaker` struct is invalid.
    pub fn clone_and_modify_creature(&self, f: impl FnOnce(&mut Creature)) -> Floor {
        let mut new_floor = self.get_floor().clone();
        let myself = new_floor
            .get_creature_list_mut()
            .entry(self.id)
            .or_insert_with(|| unreachable!());
        f(myself);
        new_floor
    }

    /// Modifies everything in a clone of the `Floor`.
    ///
    /// # Errors
    /// The passed function errors. Usually this is because an id has no creature, or no creature can be found.
    ///
    /// # Panics
    /// The `Turntaker` struct is invalid.
    pub fn clone_and_try_modify_everyone(
        &self,
        f: impl FnOnce(&mut Creature, [Option<&mut Creature>; 256]) -> Result<(), CommandError>,
    ) -> Result<Floor, CommandError> {
        let mut new_floor = self.get_floor().clone();
        let mut myself = None;
        let mut everyone_else = new_floor.get_creature_list_mut().mut_creatures_flat();
        std::mem::swap(&mut myself, &mut everyone_else[self.id as usize]);
        f(
            myself.expect("the clone of the turntaker should exist in the clone of the floor"),
            everyone_else,
        )?;
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
