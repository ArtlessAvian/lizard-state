using Godot;
using System;
using System.Collections.Generic;

public interface Action
{
    bool Do(ModelAPI api, Entity e);
}
