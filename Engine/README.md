This directory contains logic for the dungeon crawl.

Models contain Entities. Entities take turns doing Actions.
Actions cause the Model to emit a sequence of events.
Between Actions, Systems run for consistency.

There are interfaces for Actions and Systems. There should not be any implementations here, unless necessary.

This should only be written in C#. Godot should not be referenced.