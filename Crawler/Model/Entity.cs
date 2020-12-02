using Godot;
using System;

public class Entity
{
    public (int x, int y) position;
    public int nextMove = 0;

    public Species species;

    public int health = 10;

    public Entity(Species species, (int x, int y) position)
    {
        this.species = species;
        this.position = position;        
    }
}
