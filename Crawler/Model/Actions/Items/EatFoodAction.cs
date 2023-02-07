using System;
using Godot;
using Godot.Collections;
using System.Collections.Generic;

public class EatFoodAction : Action
{
    public override bool Do(Model model, Entity e)
    {
        if (e.energy < 10)
        {
            e.energy += 1;
        }
        e.nextMove += 1;
        e.hasEaten = true;
        return true;
    }

    public override bool IsValid(Model model, Entity e)
    {
        return true;
    }
}