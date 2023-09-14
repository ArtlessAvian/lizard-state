easier than trello since its only just me (for now?)

# TODO
## Gameplay Vertical slice
* Metaprogression
    * points ("science")
    * items
* Mission list
* Mission objectives
    * Human / Science
        * Mine wall tiles. (easiest)
        * Collect food/water sample.
        * Capture entity.
    * Traveler / Reputation(?)
        * Defeat entity. (easiest)
        * Meet NPC.
    * "Public relations"
        * Haul heavy item.

## Flavor vertical slice
* Talk to partner in crawl
* Walkabout Lobby
    * Talk to partner
        * Fire Emblem supports?
    * Get mission list
    * Depart on mission
    * Set items

## Make the game interesting
* Blocking as a state
* (Redo) Planar graph generation

## Make the game playable
* Danger mode
* Cancelable goto (or other "macros") in controller logic
* Expose action information
* Turn order indicator
* "z targeting" with tab. (in general, select object first, then verb)

## Low prio
* Remove movement tween in actor?
* Mission side objectives?

## Rewrites (that i shouldn't do without prep)
* View rewrite, literal 3d instead of fake 3d in 2d.
* Controller rewrite?
    * Keep no-alloc(?) style
        * Alt: no delete style?
    * Multiple entities?
    * Networked users?????

# OLD TODOS
## Edits, Fixes
* Explain attacks better (EX: Basic Throw: 50% raw, 0% combo).
* Adjust MessageLog size.
* Healthbar blocked by entity
* Dying Animation Jitter

## New Features:
* On "Goto," draw path taken, or add animation.
* Control schemes (WASD + diagonals?)
* "Are you sure?" prompt.
* Items for meta-progression.
* Incentive to explore.
* Action Sequences.

## Low Priority
* Test making AbilitySelect window dragable (like an MMO).
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