use core::fmt::Display;
use core::ops::Deref;
use core::ops::DerefMut;
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
pub struct CreatureList([Option<Rc<Creature>>; 256]);

impl CreatureList {
    pub fn new_empty() -> Self {
        CreatureList([const { None }; 256])
    }

    pub fn new_from_iter(into_iter: impl IntoIterator<Item = Creature>) -> Self {
        let mut out = Self::new_empty();
        for (slot, thing) in out.iter_mut_entries().zip(into_iter.into_iter()) {
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

    #[expect(dead_code, reason = "Eventually useful.")]
    pub fn iter_creatures(&self) -> impl Iterator<Item = &Creature> {
        self.iter_entries().flatten()
    }

    pub fn iter_indices_nonempty(&self) -> impl Iterator<Item = (u8, &Creature)> {
        (0..255)
            .zip(self.iter_entries())
            .filter_map(|(i, x)| x.map(|y| (i, y)))
    }

    /// You usually want `get_entry_mut`. This does let you search for the next empty slot.
    pub fn iter_mut_entries(&mut self) -> impl Iterator<Item = &mut Option<Rc<Creature>>> {
        self.0.iter_mut()
    }

    /// This lets you change None into Some, or the other way around.
    #[expect(dead_code, reason = "Eventually useful.")]
    pub fn get_entry_mut(&mut self, index: u8) -> &mut Option<Rc<Creature>> {
        self.iter_mut_entries()
            .nth(index as usize)
            .expect("all u8 indices are valid")
    }

    /// You usually want `get_creature_mut`.
    /// Mutating the return elements will also mutate self.
    pub fn iter_creatures_mut(&mut self) -> impl Iterator<Item = impl DerefMut<Target = Creature>> {
        self.0.iter_mut().filter_map(Option::as_mut).map(EntryMut)
    }

    /// Returns a mutable Creature.
    /// Mutating it will also mutate self.
    pub fn get_creature_mut(&mut self, index: u8) -> impl DerefMut<Target = Creature> {
        self.iter_creatures_mut()
            .nth(index as usize)
            .expect("all u8 indices are valid")
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

pub struct EntryMut<'a>(&'a mut Rc<Creature>);

impl Deref for EntryMut<'_> {
    type Target = Creature;

    fn deref(&self) -> &Self::Target {
        self.0
    }
}

impl DerefMut for EntryMut<'_> {
    fn deref_mut(&mut self) -> &mut Self::Target {
        Rc::make_mut(self.0)
    }
}
