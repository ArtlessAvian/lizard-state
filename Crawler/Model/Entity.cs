using Godot;
using Godot.Collections;
using System;

public class Entity
{
    public int id;

    public (int x, int y) position;
    public int nextMove = 0;

    public Species species;
    public AI ai;

    public int health;
    public bool stunned;
    public bool downed = false;

    public int team;
    public bool providesVision;
    public bool dirtyVision; // hehe

    public Entity(Species species, (int x, int y) position, int team)
    {
        this.species = species;
        this.position = position;
        this.team = team;

        // HACK.
        providesVision = team == 0;

        this.health = species.maxHealth;
        this.ai = new AI();
    }

    public void ResetCombo()
    {
        this.stunned = false;
    }

    public void GetAttacked(ModelAPI api, AttackResult result, int attackerID)
    {
        if (result.damage == 0)
        {
            api.NewEvent(new ModelEvent(attackerID, "Miss", result, this.id));
        }
        else
        {
            api.NewEvent(new ModelEvent(attackerID, "Hit", result, this.id));

            this.health -= result.damage;

            if (this.health <= 0)
            {
                api.NewEvent(new ModelEvent(id, "Downed"));
                this.downed = true;
                this.nextMove = -1;
            }
            else if (result.stuns)
            {
                this.nextMove = Math.Max(result.stunUntil, this.nextMove);
                this.stunned = true;
            }

        }
    }

    // public void TakeDamage(AttackResult roll)
    // {
    //     this.health -= roll.damage;
    //     if (this.health <= 0)
    //     {
    //         this.downed = true;
    //         this.nextMove = -1;
    //     }
    //     else if (roll.hit)
    //     {
    //         this.nextMove = Math.Max(roll.stunUntil, this.nextMove);
    //         this.stunned = true;
    //     }
    // }

    public Dictionary SaveToDictionary()
    {
        Dictionary dict = new Dictionary();
        dict["species"] = species.ResourcePath;
        dict["x"] = position.x;
        dict["y"] = position.y;
        dict["nextMove"] = nextMove;
        dict["health"] = health;
        dict["stunned"] = stunned;
        dict["downed"] = downed;
        dict["team"] = team;
        dict["providesVision"] = providesVision;
        // dict["AI"] = ai.SaveToDict();
        return dict;
    }

    public Entity(Dictionary dict)
    {
        this.species = GD.Load<Species>((string)dict["species"]);
        this.position.x = (int)dict["x"];
        this.position.y = (int)dict["y"];
        this.nextMove = (int)dict["nextMove"];
        this.health = (int)dict["health"];
        this.stunned = (bool)dict["stunned"];
        this.downed = (bool)dict["downed"];
        this.team = (int)dict["team"];
        this.providesVision = (bool)dict["providesVision"];

        // this.ai = new AI((Dictionary)dict["AI"]);
    }
}
