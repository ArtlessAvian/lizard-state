using Godot;
using System;
using System.Collections.Generic;

// Should be reusuable, though not always. :/
public interface Action
{
    bool Do(ModelAPI api, Entity e);
}
