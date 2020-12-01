using Godot;
using System;

public class Entity
{
    public (int x, int y) position;
    public int health = 10;
    public Species species;

    public Entity(Species species, (int x, int y) position)
    {
        this.species = species;
        this.position = position;        
    }
}
