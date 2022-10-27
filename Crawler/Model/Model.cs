using System.Collections.Generic;
using Godot;
using Godot.Collections;

/// <summary>
/// Represents one floor. (Each floor acts independently of each other!)
/// Stores the game state and handles turn taking.
/// Remember to keep view information in the view counterpart!
/// </summary>
public partial class Model : Resource
{
    // TODO: distinguish between "win" and "lose"
    [Export] public bool done = false; // set to true when done.

    // Everything is saved!!
    [Export] public int time = 0;

    [Export] public CrawlerMap map = null;
    [Export] public List<Resource> systems = new List<Resource>();

    [Export] private List<Entity> entities = new List<Entity>();
    [Export] private List<FloorItem> floorItems = new List<FloorItem>();

    [Signal]
    public delegate void NewEvent(Dictionary ev);

    public Model()
    {
        map = (CrawlerMap)GD.Load<CSharpScript>("res://Crawler/Model/CrawlerMap.cs").New();
    }

    public void AddEntity(Entity e)
    {
        e.id = entities.Count;
        entities.Add(e);

        this.CoolerApiEvent(-1, "Create", e, e.id);

        RunSystems();
    }

    public void AddFloorItem(FloorItem item)
    {
        item.id = floorItems.Count;
        floorItems.Add(item);

        this.CoolerApiEvent(-1, "CreateItem", item, item.id);
    }

    /// <summary>
    /// Sets the next action for the player character.
    /// <returns> true if its the player turn AND action is valid AND (reasonable OR forced)  </returns>
    /// </summary>
    // I wrote this to avoid code duplication between player and entity actions.
    // I don't think I did that correctly.
    public bool SetPlayerAction(Action action, bool force = false)
    {
        Entity e = NextEntity();
        if (!e.isPlayer) { return false; }

        if (!action.IsValid(this, e)) { return false; }
        // if (!action.IsReasonable() && !force) { return false; }

        e.queuedAction = action;
        return true;
    }

    /// <summary>
    /// Attempts to run the next entity's action.
    /// <returns> true if entity has action </returns>
    /// </summary>
    // 
    public bool DoStep()
    {
        if (done) { return false; }

        Entity e = NextEntity();
        PassTime(e.nextMove);

        // Get the next action.
        Action action = GetAction(e);

        if (action == null)
        {
            if (!e.isPlayer)
            {
                GD.PrintErr($"{e.species.displayName} has no ai or ai returned null!");
                GD.PrintErr("waiting instead..");
                action = new MoveAction().SetTargetRelative((0, 0));
            }
            else
            {
                CoolerApiEvent(e.id, "YourTurn");
                return false;
            }
        }

        e.queuedAction = null; // null out if used.
        bool success = action.Do(this, e);
        if (!success)
        {
            GD.Print($"{e.species.displayName} tried {action.GetType().ToString()} and failed!");
            e.nextMove += 1;
        }

        RunSystems();
        return true;
    }

    private Action GetAction(Entity e)
    {
        switch (e.state)
        {
            case Entity.EntityState.OK:
                if (e.queuedAction != null)
                {
                    return e.queuedAction;
                }
                break;
            case Entity.EntityState.STUN:
                return (Action)GD.Load<CSharpScript>("res://Crawler/Model/Actions/StunRecoveryAction.cs").New();
            case Entity.EntityState.KNOCKDOWN:
                return (Action)GD.Load<CSharpScript>("res://Crawler/Model/Actions/KnockdownWakeupAction.cs").New();
            default:
                break;
        }

        if (!e.isPlayer)
        {
            return e.species?.ai?.GetMove(this, e);
        }
        return null;
    }

    /// <summary>
    /// Runs all the systems, (usually after every move).
    /// This could be more efficient but whatever.
    /// </summary>
    private void RunSystems()
    {
        foreach (Resource resource in systems)
        {
            if (resource is CrawlerSystem sys)
            {
                sys.Run(this);
            }
        }
    }

    public Entity NextEntity()
    {
        Entity result = GetEntity(0);
        foreach (Entity e in GetEntities())
        {
            if (e.state == Entity.EntityState.UNALIVE) { continue; }
            if (e.nextMove < result.nextMove)
            {
                result = e;
            }
        }
        return result;
    }

    public void PassTime(int finalTime)
    {
        int delta = finalTime - time;
        time = finalTime;
    }

    // todo: rename this lmao
    // [Obsolete]
    public void CoolerApiEvent(int subject, string action, object args = null, int @object = -1)
    {
        CoolerApiEvent(new Godot.Collections.Dictionary()
        {
            {"subject", subject},
            {"action", action},
            {"args", args},
            {"object", @object}
        });
    }

    public void CoolerApiEvent(Godot.Collections.Dictionary @event)
    {
        @event.Add("timestamp", time);

        // For each system, decorate the event.
        // foreach (CrawlerSystem system in GetNode("Systems").GetChildren())
        // {
        //     system.ProcessEvent(this, @event);
        // }

        // Send the event to the view, if the player('s team) sees it.
        this.EmitSignal("NewEvent", @event);
        // GD.Print(@event);

        // For each system, react to the event.
        // (Skill procs, or something? could be fun)
        // foreach (CrawlerSystem system in GetNode("Systems").GetChildren())
        // {
        //     system.ProcessEvent(this, @event);
        // }
    }

    public void Debug(string message)
    {
        CoolerApiEvent(-1, "Debug", message);
    }
}
