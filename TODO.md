easier than trello since its only just me (for now?)

# TODO
## Edits, Fixes
* Make Energy visible.
* Test making AbilitySelect window dragable (like an MMO).
* Add prompt to denote AbilityAiming (which ability is selected, or even that you've selected something).
* Explain attacks better (EX: Basic Throw: 50% raw, 0% combo).
* Draw ranges for abilities.
* Adjust MessageLog size.
* Add turn indicator
* Healthbar blocked by entity

## New Features:
* On "Goto," draw path taken, or add animation.
* Control schemes (WASD + diagonals?)
* "Are you sure?" prompt.
* Multiple caves.
* Heal between caves.
* Items for meta-progression.
* Incentive to explore.

## Low Priority
* Limit dash with LOS check, unoccupied tile.
* Experiment more with friendly fire.
* Find art. Find sfx.

## Fun Stuff
* add damage on dashing through people
* make partner give tutorial
* check for partner nearby (if alive) before leaving

## Redos
* Redo vision system. (Buggy, awkward.)
* Redo view. (In GDScript?)

# Questions to ask people
* The usual:
    * How were the controls?
    * Anything that was unclear?
    * What motivated you? What was fun?
* Should the map be bigger? smaller?
* Should the fov be bigger? smaller?
* Did the your partner or the enemies (the AI) do something dumb/unexpected?

# Notes to self
* The view has perfect information
    * The view /can/ show the true state of the game if it wanted to.
    * Having the model hide information from the view would be cool
        * But also hard.

* The game does not like hot-reloading
    * This would have been really nice.
    * I blame the model being in the scene tree.

* The model is in the scene tree.
    * The model is dependent on Godot as a consequence.
        * I also use Godot as a library at times.
            * TileMaps as sparse int arrays
            * Resources as data files
    * 
