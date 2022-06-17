using Godot;
using Godot.Collections;
using System;
using System.Collections.Generic;

/// <summary>
/// Plain(ish?) object. Should contains little logic.
/// </summary>
// TODO: Rework Entities to have logic and to use callbacks.
// TODO: Make Entity a class of structs?
public class Entity : Resource
{
    [Export] public int id;
    [Export] public Species species;

    [Export] public bool isPlayer = false;

    // Godot doesn't like serializing tuples, and I don't want to use Vector2.
    // So this is what we have to do.
    [Export] public int positionX;
    [Export] public int positionY;
    public (int x, int y) position
    {
        get { return (positionX, positionY); }
        set { positionX = value.x; positionY = value.y; }
    }
    [Export] public bool visibleToPlayer = false;

    [Export] public int nextMove = 0;
    public Action queuedAction;

    [Export] public int health;
    [Export] public bool stunned; // TODO: Rework all this.
    [Export] public int comboCounter; // Undizzy.
    [Export] public bool downed = false;

    [Export] public int energy = 10;
    public InventoryItem inventory = null;

    [Export] public int team;
    [Export] public bool providesVision;
    [Export] public bool dirtyVision; // hehe

    public Entity() { }

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
        this.comboCounter = 0;
        this.stunned = false;
    }

    // public void TakeDamage(int damage) // add callback param
    // {
    //     this.health -= damage;
    //     this.queuedAction = null;

    //     if (this.health <= 0)
    //     {
    //         this.downed = true;
    //     }
    // }

    // public void TakeDamage(AttackResult result)
    // {
    //     this.health -= result.damage;

    //     if (this.health <= 0)
    //     {
    //         this.downed = true;
    //         this.nextMove = -1;
    //     }
    //     else if (result.stuns)
    //     {
    //         this.comboCounter += 1;

    //         this.nextMove = Math.Max(this.nextMove, result.stunUntil - (comboCounter > 3 ? (comboCounter - 3) * 5 : 0));
    //         this.stunned = true;
    //         this.queuedAction = null;
    //     }
    // }

    public Dictionary SaveToDictionary()
    {
        Dictionary dict = new Dictionary();
        dict["species"] = species.ResourcePath;
        dict["isPlayer"] = isPlayer;
        dict["positionX"] = positionX;
        dict["positionY"] = positionY;
        dict["nextMove"] = nextMove;

        if (queuedAction != null)
        {
            dict["queuedActionDictionary"] = queuedAction.SaveToDictionary();
            dict["queuedAction"] = queuedAction?.GetType().ToString();
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
        this.isPlayer = (bool)dict["isPlayer"];
        this.positionX = (int)dict["positionX"];
        this.positionY = (int)dict["positionY"];
        this.nextMove = (int)dict["nextMove"];

        if (dict.Contains("queuedAction"))
        {
            GD.Print(dict["queuedActionDictionary"]);
            Dictionary actionDict = dict["queuedActionDictionary"] as Dictionary;
            GD.Print(actionDict.GetType().ToString());
            // TODO: make sure everything that extends action serializes what it needs to save.
            this.queuedAction = (Action)Activator.CreateInstance(Type.GetType((string)dict["queuedAction"]), actionDict);
        }

        this.health = (int)dict["health"];
        this.stunned = (bool)dict["stunned"];
        this.downed = (bool)dict["downed"];
        this.energy = (int)dict["energy"];
        this.team = (int)dict["team"];
        this.providesVision = (bool)dict["providesVision"];
    }
}
