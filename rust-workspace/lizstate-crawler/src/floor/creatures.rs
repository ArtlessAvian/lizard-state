use core::fmt::Display;
use core::ops::IndexMut;
use std::rc::Rc;

use crate::creature::Creature;

#[derive(Debug, Clone, Copy, PartialEq, Eq, PartialOrd, Ord)]
#[must_use]
pub struct Turn {
    round: u32,
    order: u8,
}

impl Turn {
    pub fn coming_round_for(self, other: u8) -> u32 {
        self.round + u32::from(self.order > other)
    }

    pub fn skip_rounds(self, count: u32) -> Turn {
        Turn {
            round: self.round + count,
            order: self.order,
        }
    }
}

impl Display for Turn {
    fn fmt(&self, f: &mut std::fmt::Formatter<'_>) -> std::fmt::Result {
        f.debug_tuple("")
            .field(&self.round)
            .field(&self.order)
            .finish()
    }
}

/// A list of creatures. Clones with persistence.
///
/// Persistence allows previous floors to share data with each other.
/// Almost all creatures do not change over a turn.
///
/// Indices are u8s. All u8s are valid.
#[derive(Debug, Clone)]
#[must_use]
pub struct CreatureList([Option<Rc<Creature>>; 256]);

impl CreatureList {
    pub fn new_empty() -> Self {
        CreatureList([const { None }; 256])
    }

    pub fn new_from_iter(into_iter: impl IntoIterator<Item = Creature>) -> Self {
        let mut out = Self::new_empty();
        for (slot, thing) in out.0.iter_mut().zip(into_iter.into_iter()) {
            *slot = Some(Rc::new(thing));
        }
        out
    }

    pub fn iter_entries(&self) -> impl Iterator<Item = Option<&Creature>> {
        self.0
            .each_ref()
            .map(Option::as_ref)
            .map(|opt| opt.map(Rc::as_ref))
            .into_iter()
    }

    pub fn iter_creatures(&self) -> impl Iterator<Item = &Creature> {
        self.iter_entries().flatten()
    }

    pub fn iter_indices_nonempty(&self) -> impl Iterator<Item = (u8, &Creature)> {
        (0..255)
            .zip(self.iter_entries())
            .filter_map(|(i, x)| x.map(|y| (i, y)))
    }

    /// Returns a mutable Creature.
    /// This eagerly mutates the Rc in self.
    pub fn get_creature_mut(&mut self, index: u8) -> Option<&mut Creature> {
        self.0.index_mut(index as usize).as_mut().map(Rc::make_mut)
    }

    /// Overwrites a slot with a creature, then gets a mutable reference to the contained creature.
    ///
    /// See `Self::get_creature_mut_or_insert` to preserve the contents.
    pub fn set_creature_then_get_mut(&mut self, index: u8, creature: &Creature) -> &mut Creature {
        let rc = self
            .0
            .index_mut(index as usize)
            .get_or_insert_with(|| Rc::new(Creature::new_garbage()));

        let mut_ref = Rc::make_mut(rc);
        *mut_ref = creature.clone();
        mut_ref
    }

    /// Gets a creature if already present, otherwise inserts the argument.
    /// In the "already present" path, this avoids an `Option::unwrap`.
    ///
    /// See `Self::set_creature_then_get_mut` to eagerly overwrite.
    pub fn get_creature_mut_or_insert(&mut self, index: u8, creature: &Creature) -> &mut Creature {
        let rc = self
            .0
            .index_mut(index as usize)
            .get_or_insert_with(|| Rc::new(creature.clone()));

        Rc::make_mut(rc)
    }

    pub fn iter_turn_order(&self) -> impl Iterator<Item = (u8, Turn, &Creature)> {
        let mut vec = self
            .iter_indices_nonempty()
            .filter_map(|(id, x)| {
                x.get_round()
                    .map(|round| (id, Turn { round, order: id }, x))
            })
            .collect::<Vec<_>>();

        vec.sort_unstable_by_key(|x| x.1);
        vec.into_iter()
    }
}
