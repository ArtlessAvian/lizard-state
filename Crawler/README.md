This directory contains the Godot Scene representing one Model (Engine/Model.cs).
It also contains data needed to represent the BaseGame.

The View does not handle user input. It represents the model, and using events emit from the model, animates any changes.

The Controller handles user input. Currently, it is synchronous, relying on enabling callbacks from Godot.
It is also stack based. When any input is successfully sent to the Model, the state returns to the start.

<!-- TODO: consider "epsilon transitions" from the start (or all) to special states. -->