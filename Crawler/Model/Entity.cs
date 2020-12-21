using Godot;
using Godot.Collections;
using System;

public class Entity
{
    public (int x, int y) position;
    public int nextMove = 0;

    public Species species;
    public CrawlerAI ai;

    public int health = 10;

    public Entity(Species species, (int x, int y) position)
    {
        this.species = species;
        this.position = position;
        this.ai = new AI(species.aiType);
    }

    public Dictionary SaveToDictionary()
    {
        Dictionary dict = new Dictionary();
        dict["species"] = species.ResourcePath;
        dict["x"] = position.x;
        dict["y"] = position.y;
        dict["nextMove"] = nextMove;
        dict["health"] = health;
        return dict;
    }

    public Entity(Dictionary dict)
    {
        this.species = GD.Load<Species>((string)dict["species"]);
        this.position.x = (int)dict["x"];
        this.position.y = (int)dict["y"];
        this.nextMove = (int)dict["nextMove"];
        this.health = (int)dict["health"];

        GD.Print(this.position.x);
    }
}
