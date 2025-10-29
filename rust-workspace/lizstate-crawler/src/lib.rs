//! A traditional roguelike, dungeon crawl engine. Tailored for Lizard State.
//!
//! Besides the usual features, there are plans for:
//! * easy undo, save, and load.
//! * delayed commands.
//! * stun and whatnot.
//!
//! The *game* design for those decisions is out of scope here.
//! TL;DR it's sort of fighting game like.

#![warn(clippy::pedantic)]
#![warn(clippy::allow_attributes_without_reason)]

/// `Tile`s, positions that are gridlike, that form a funky `Map`.
///
/// `Map`s and their `Tile`s can be projected into a grid, but the original is not the plane.
pub mod spatial;

/// The 'Entity' trait describes things that should be drawn to the screen.
pub mod entity;

/// A `Creature` is a concrete type implementing `Entity`.
///
/// They have a few basic mutable operations.
/// They are also partially ordered by turn order.
pub mod creature;

/// A `Floor` contains `Creature`s, a `Map`, and other resources/systems.
///
/// `Floor`s can be cloned easily for undo, quicksave, and quickload.
/// `Floor`s can also be serialized and deserialized.
///
/// In the context of a `Floor`, `Now` is the smallest `Turn`.
/// A `Subject` is a `Floor` and its turn taker.
pub mod floor;

/// The `Command` trait describe how a `Floor` changes in a turn and produces a `Log`.
pub mod commands;

/// Reexports from the other modules. The usual stuff.
pub mod prelude;
