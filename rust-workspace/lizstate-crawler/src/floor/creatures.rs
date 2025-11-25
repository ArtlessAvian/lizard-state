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

    /// Overwrites a slot with a creature, then gets a mutable reference to the contained creature.
    ///
    /// See `Self::get_creature_mut_or_insert` to preserve the contents.
    pub fn set_creature_then_get_mut(&mut self, index: u8, creature: Creature) -> &mut Creature {
        let yeah = self.entry(index);
        yeah.and_modify(|x| *x = creature.clone())
            .or_insert_with(|| creature)
    }

    /// Gets a creature if already present, otherwise inserts the argument.
    /// In the "already present" path, this avoids an `Option::unwrap`.
    ///
    /// See `Self::set_creature_then_get_mut` to eagerly overwrite.
    pub fn get_creature_mut_or_insert(&mut self, index: u8, creature: &Creature) -> &mut Creature {
        let yeah = self.entry(index);
        yeah.or_insert_with(|| creature.clone())
    }

    pub fn iter_turn_order(&self) -> impl Iterator<Item = (u8, Turn, &Creature)> {
        let mut vec = self
            .iter()
            .filter_map(|(id, x)| {
                x.get_round()
                    .map(|round| (id, Turn { round, order: id }, x))
            })
            .collect::<Vec<_>>();

        vec.sort_unstable_by_key(|x| x.1);
        vec.into_iter()
    }
}

/// Map interface.
impl CreatureList {
    pub fn creatures_flat(&self) -> [Option<&Creature>; 256] {
        self.0
            .each_ref()
            .map(Option::as_ref)
            .map(|opt| opt.map(Rc::as_ref))
    }

    pub fn mut_creatures_flat(&mut self) -> [Option<&mut Creature>; 256] {
        self.0
            .each_mut()
            .map(Option::as_mut)
            .map(|opt: Option<&mut Rc<Creature>>| opt.map(Rc::make_mut))
    }

    pub fn entries_flat(&mut self) -> [Entry<'_>; 256] {
        self.0.each_mut().map(|x| {
            if let Some(rc) = x {
                Entry::Occupied(OccupiedEntry(Rc::make_mut(rc)))
            } else {
                Entry::Vacant(VacantEntry(x))
            }
        })
    }

    pub fn get(&self, index: u8) -> Option<&Creature> {
        self.0[index as usize].as_ref().map(Rc::as_ref)
    }

    pub fn get_mut(&mut self, index: u8) -> Option<&mut Creature> {
        self.0[index as usize].as_mut().map(Rc::make_mut)
    }

    pub fn creatures(&self) -> impl Iterator<Item = &Creature> {
        self.creatures_flat().into_iter().flatten()
    }

    pub fn mut_creatures(&mut self) -> impl Iterator<Item = &mut Creature> {
        self.mut_creatures_flat().into_iter().flatten()
    }

    pub fn insert(&mut self, index: u8, creature: Creature) -> Option<Creature> {
        let out = self.0.index_mut(index as usize).replace(Rc::new(creature));
        out.map(Rc::unwrap_or_clone)
    }

    pub fn remove(&mut self, index: u8) -> Option<Creature> {
        let out = self.0.index_mut(index as usize).take();
        out.map(Rc::unwrap_or_clone)
    }

    pub fn entry(&mut self, index: u8) -> Entry<'_> {
        let yeah = self.0.index_mut(index as usize);

        if let Some(rc) = yeah {
            Entry::Occupied(OccupiedEntry(Rc::make_mut(rc)))
        } else {
            Entry::Vacant(VacantEntry(yeah))
        }
    }

    pub fn iter(&self) -> impl Iterator<Item = (u8, &Creature)> {
        (0..=255u8)
            .zip(self.creatures_flat())
            .filter_map(|(id, opt)| opt.map(|x| (id, x)))
    }

    pub fn iter_mut(&mut self) -> impl Iterator<Item = (u8, &mut Creature)> {
        (0..=255u8)
            .zip(self.mut_creatures_flat())
            .filter_map(|(id, opt)| opt.map(|x| (id, x)))
    }
}

#[must_use]
pub enum Entry<'a> {
    Vacant(VacantEntry<'a>),
    Occupied(OccupiedEntry<'a>),
}

pub struct VacantEntry<'a>(&'a mut Option<Rc<Creature>>);
pub struct OccupiedEntry<'a>(&'a mut Creature);

impl<'a> Entry<'a> {
    pub fn and_modify(mut self, f: impl FnOnce(&mut Creature)) -> Self {
        if let Entry::Occupied(OccupiedEntry(x)) = &mut self {
            f(x);
        }
        self
    }

    pub fn insert_entry(self, creature: Creature) -> OccupiedEntry<'a> {
        match self {
            Entry::Vacant(x) => OccupiedEntry(x.insert(creature)),
            Entry::Occupied(x) => {
                *x.0 = creature;
                x
            }
        }
    }

    pub fn or_insert(self, creature: Creature) -> &'a mut Creature {
        match self {
            Entry::Vacant(x) => x.insert(creature),
            Entry::Occupied(OccupiedEntry(x)) => x,
        }
    }

    pub fn or_insert_with(self, f: impl FnOnce() -> Creature) -> &'a mut Creature {
        match self {
            Entry::Vacant(VacantEntry(mut_none)) => Rc::make_mut(mut_none.insert(Rc::new(f()))),
            Entry::Occupied(OccupiedEntry(x)) => x,
        }
    }
}

impl<'a> VacantEntry<'a> {
    fn insert(self, creature: Creature) -> &'a mut Creature {
        let inner = self.0.insert(Rc::new(creature));
        Rc::make_mut(inner)
    }
}

impl FromIterator<(u8, Creature)> for CreatureList {
    fn from_iter<T: IntoIterator<Item = (u8, Creature)>>(iter: T) -> Self {
        let mut out = Self::new_empty();
        for (index, creature) in iter {
            let _ = out.insert(index, creature);
        }
        out
    }
}

impl FromIterator<Creature> for CreatureList {
    fn from_iter<T: IntoIterator<Item = Creature>>(iter: T) -> Self {
        (0..=255u8).zip(iter).collect()
    }
}
