/// Common enums. Path definition.
pub mod relative;

/// The trait for positions.
pub mod grid;
/// The trait for partial functions of positions.
pub mod map;

/// The `Grid` that `flatten`s into itself, and a map for them.
pub mod coords;

/// A `Grid` made from a outer `Grid` and a smaller bounded `Grid`.
pub mod chunks;
