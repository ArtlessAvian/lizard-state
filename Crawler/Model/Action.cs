using Godot;
using System;
using System.Collections.Generic;

/// <summary>
/// The command pattern. Create action objects, use, then throw away.
/// The action can also be "unreasonable."
/// </summary>
public interface Action
{
    // Do will ALWAYS run, no matter what.
    bool Do(ModelAPI api, Entity e);

    // Checks if the action is reasonable.
    // bool IsValid(ModelAPI api, Entity e);

    // Gives a list of reasons this may be a bad move.
    // string[] GetWarnings(ModelAPI api, Entity e);
}
