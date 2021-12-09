# Lizard State
A moody roguelike with meaningful combat decisions and a partner.

Playable in browser at artlessavian.github.io/lizard-state

<!-- # Contributing
Please describe your commits and more importantly,
/why/ you made the decisions you did.

(its probably just me contributing) -->


# Development Notes

* Don't decouple everything so much.
    * It's not a REST server
    * (Statelessness is good though, but I might break that eventually.)

* The view must be initialized before the model is.
    * (I'm trying to move away from this.)

* The view has perfect information.
    * The model tells the view *everything*.
    * The view can show the true state of the game if it wanted to.
    * Having the model hide information from the view would be cool.
        * But also hard.

* The controller plays the model, not the view.

* AI is not smart.
    * AI is not general either.
    * Hardcode each enemy type with their own AI.

* The game does not like hot-reloading.
    * This would have been really nice.
    * It works-ish, but I did not develop with it in mind.
    * It crashes often.

* Godot can be used as a library.
    * TileMaps as sparse int arrays (createse orphan nodes)
    * Resources as data files (naturally)