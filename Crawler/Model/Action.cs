using Godot;
using System;
using System.Collections.Generic;

// Interface, also serves as a "null action"
public interface Action
{
    bool Do(ModelAPI api, List<ModelEvent> eventQueue, Entity e);
}
