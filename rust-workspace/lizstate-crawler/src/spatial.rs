/// Common enums. Path definition.
pub mod relative;

/// The trait for positions and a boring implementation.
pub mod grid;
/// The trait for partial functions of positions and a boring implementation.
pub mod map;

/// A `Grid` made from a outer `Grid` and a smaller bounded `Grid`.
pub mod chunks;

/// Chunks, connected by paths (the free group).
pub mod paths_and_chunks;
