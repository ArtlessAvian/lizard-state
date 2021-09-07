using Godot;
using Godot.Collections;
using System;
using System.Collections.Generic;

/// <summary>
/// Plain(ish?) object. Should contains little logic.
/// </summary>
// TODO: Rework Entities to have logic and to use callbacks.
// TODO: Make Entity a class of structs?
public class Entity : Node
{
    [Export] public int id;
    [Export] public Species species;

    private Vector2 _position = new Vector2();
    public (int x, int y) position
    {
        get { return ((int)_position.x, (int)_position.y); }
        set { _position.x = value.x; _position.y = value.y; }
    }
    public int nextMove = 0;

    public Action queuedAction;

    public int health;
    public bool stunned; // TODO: Rework all this.
    public bool downed = false;

    public int energy = 10;

    public int team;
    public bool providesVision;
    public bool dirtyVision; // hehe

    public Entity() {}

    public override void _EnterTree()
    {
        if (species == null)
        {
            GD.Print($"Entity {id} missing species!");
            this.SetSpecies(GD.Load<Species>("res://Crawler/Model/Species/Enemy.tres"));
        }
    }

    public void SetSpecies(Species species)
    {
        this.species = species;
        this.health = species.maxHealth;
    }

    public void SetTeam(int team)
    {
        this.team = team;
        this.providesVision = team == 0;
    }

    public void ResetCombo()
    {
        this.stunned = false;
    }

    public void GetAttacked(AttackResult result)
    {
        this.health -= result.damage;
        
        if (this.health <= 0)
        {
            this.downed = true;
            this.nextMove = -1;
        }
        else if (result.stuns)
        {
            this.nextMove = Math.Max(result.stunUntil, this.nextMove);
            this.stunned = true;
            this.queuedAction = null;
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
        dict["position"] = _position;
        dict["nextMove"] = nextMove;

        if (queuedAction != null)
        {
            dict["queuedAction"] = queuedAction?.GetType().ToString();
            dict["queuedActionTarget"] = queuedAction.GetTargetPos(position);
        }

        dict["health"] = health;
        dict["stunned"] = stunned;
        dict["downed"] = downed;
        dict["energy"] = energy;
        dict["team"] = team;
        dict["providesVision"] = providesVision;
        // dict["AI"] = ai.SaveToDict();
        return dict;
    }

    public Entity(Dictionary dict)
    {
        this.species = GD.Load<Species>((string)dict["species"]);
        this._position = (Vector2)dict["position"];
        this.nextMove = (int)dict["nextMove"];

        if (dict.Contains("queuedAction"))
        {
            this.queuedAction = (Action)Activator.CreateInstance(Type.GetType((string)dict["queuedAction"]));
            this.queuedAction.SetTarget(((int x, int y))dict["queuedActionTarget"]);
        }

        this.health = (int)dict["health"];
        this.stunned = (bool)dict["stunned"];
        this.downed = (bool)dict["downed"];
        this.energy = (int)dict["energy"];
        this.team = (int)dict["team"];
        this.providesVision = (bool)dict["providesVision"];
    }
}
