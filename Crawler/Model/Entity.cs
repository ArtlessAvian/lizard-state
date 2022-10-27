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
    public enum EntityState
    {
        OK, STUN, KNOCKDOWN, UNALIVE, EXITED
    }

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

    [Export] public EntityState state = EntityState.OK;
    [Export] public Action queuedAction;

    // Imagine a rust enum with OK(Option<Action>), that being queuedAction?
    // If I were a braver person I'd use this everywhere.
    public (EntityState state, Action action) stateOrQueuedAction
    {
        get { return (state, queuedAction); }
        set { state = value.state; queuedAction = value.state == EntityState.OK ? value.action : null; }
    }

    [Export] public int health;
    [Export] public int energy = 10;

    [Export] public InventoryItem inventory = null;

    [Export] public int team;
    [Export] public bool providesVision;

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

    public void DelayNextMove(int nTurns, int time, int nowId)
    {
        // "lose {nTurns} turns." The last term ensures that lower ids lose their turn correctly.
        int stunUntil = time + nTurns + (this.id < nowId ? 1 : 0);
        this.nextMove = Math.Max(this.nextMove, stunUntil);
    }

    public void StunForTurns(int nTurns, int time, int nowId)
    {
        DelayNextMove(nTurns, time, nowId);
        stateOrQueuedAction = (EntityState.STUN, null);
    }

    public void KnockdownForTurns(int nTurns, int time, int nowId)
    {
        DelayNextMove(nTurns, time, nowId);
        stateOrQueuedAction = (EntityState.KNOCKDOWN, null);
    }
}
