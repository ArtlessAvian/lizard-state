using Godot;
using System;

public class Entity
{
    public (int x, int y) position;
    public int health = 10;
    public Species species;

    public Entity()
    {
        position.x = (int)(GD.Randi() % 3);
        position.y = (int)(GD.Randi() % 3);
    }
}
