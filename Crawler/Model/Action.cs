using Godot;
using System;
using System.Collections.Generic;

/// <summary>
/// The command pattern. Create action objects, use, then throw away.
/// The action can fail.
/// The action can also be "unreasonable."
/// </summary>
public interface Action
{
    bool Do(ModelAPI api, Entity e);
    // TODO: function to check valid
    // TODO: function to check reasonability
}
