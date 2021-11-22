# Lizard State
A moody roguelike with meaningful combat decisions and a partner.

<!-- # Contributing
Please describe your commits and more importantly,
/why/ you made the decisions you did.

(its probably just me contributing) -->


# Development Notes

* Don't decouple everything so much.
    * It's not a REST server

* The view has perfect information
    * The model tells the view *everything*.
    * The view can show the true state of the game if it wanted to.
    * Having the model hide information from the view would be cool.
        * But also hard.

* The controller plays the model, not the view.
    * TODO: pivot towards this.

* The game does not like hot-reloading
    * This would have been really nice.
    * I blame C# and casting things to variant.

* Godot can be used as a library
    * TileMaps as sparse int arrays
    * Resources as data files (naturally)