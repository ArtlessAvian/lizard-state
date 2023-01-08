using Godot;
using System;
using System.Collections.Generic;

// Is this useful? No. Good for debugging.
public class FallAsleepAction : Action
{
    public override bool Do(Model model, Entity e)
    {
        e.nextMove += 1000;
        return true;
    }

    public override bool IsValid(Model model, Entity e)
    {
        return true;
    }

    public override IEnumerable<string> GetWarnings(Model model, Entity e)
    {
        yield return "Oh worm?";
    }
}
