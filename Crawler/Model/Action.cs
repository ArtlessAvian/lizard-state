using Godot;
using System;
using System.Collections.Generic;

/// <summary>
/// The command pattern. Create action objects, use, then throw away.
/// The action can also be "unreasonable."
/// </summary>
public interface Action
{
    // Do fails if IsValid is false. This will waste a turn and might print a funny message.
    bool Do(ModelAPI api, Entity e);

    // Checks if the action is reasonable.
    // bool IsValid(ModelAPI api, Entity e);

    // Gives a list of reasons this may be a bad move.
    // string[] GetWarnings(ModelAPI api, Entity e);
}
