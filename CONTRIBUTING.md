# Tooling
We use the `pre-commit` framework to lint and do quick checks.
It's nice to run locally before pushing, but it is not necessary.
Avoid using `language: system` so everything *could* run on `pre-commit.ci`.
We don't use it as of writing, but we could.

For building and testing, we use `just` and Github Actions.
We want to put as little as possible inside Github Actions since it's awkward.
Github Actions can be run locally with `act`, if needed.
`cargo`, `godot`, and `blender` are assumed to be in the path.
There's a container that does that for you.

There's a Godot `EditorPlugin` to compile the rust gdextension before the game is run.
It also helps compile the rust gdextension before the game is exported.
