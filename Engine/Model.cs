using System.Collections.Generic;
using System.Linq;
using Godot;
using Godot.Collections;

namespace LizardState.Engine
{
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
        [Export] public List<CrawlerSystem> systems = new List<CrawlerSystem>();

        [Export] private List<Entity> entities = new List<Entity>();
        [Export] private List<FloorItem> floorItems = new List<FloorItem>();

        [Signal]
        public delegate void NewEvent(Dictionary ev);

        // Create an empty model, which is "valid" but its unclear what to decorate.
        // Prefer using the constructor.
        public Model()
        {
            map = (CrawlerMap)GD.Load<CSharpScript>("res://Engine/CrawlerMap.cs").New();
        }

        public Model(CrawlerMap map)
        {
            this.map = map;
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
        public bool SetPlayerAction(CrawlAction action, bool force = false)
        {
            Entity e = NextEntity();
            if (!e.isPlayer) { return false; }

            if (!action.IsValid(this, e)) { return false; }
            if (!force && action.GetWarnings(this, e).Any())
            {
                e.needsConfirmAction = action;
                return false;
            }

            e.needsConfirmAction = null;
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

            foreach (CrawlAction action in GetActions(e))
            {
                bool success = action.Do(this, e);
                if (success)
                {
                    RunSystems();
                    return true;
                }
            }

            if (e.isPlayer)
            {
                CoolerApiEvent(e.id, "YourTurn");
                return false;
            }
            else
            {
                GD.PrintErr($"{e.species.displayName} yielded no acceptable move. Waiting instead...");
                // Sort of expensive to rerun, but it shouldn't happen often.
                var actions = e.species?.ai?.GetMoves(this, e).Select(tup => tup.Item1).ToList();
                GD.PrintErr($"{actions.Count} ai actions: ", string.Join(", ", actions.Select(a => a.GetType().ToString())));

                new MoveAction().SetTargetRelative(Vector2i.ZERO).Do(this, e);
                RunSystems();
                return true;
            }
        }

        private IEnumerable<CrawlAction> GetActions(Entity e)
        {
            // honestly not a big fan of "states" if /all/ they do is force an automatic recovery action.
            // something i'd like to try is if states restricted the available actions.
            // breaks statelessness in the berlin interpretation but whatever. its already broken anyways.
            switch (e.state)
            {
                case Entity.EntityState.OK:
                    if (e.queuedAction is CrawlAction temp)
                    {
                        e.queuedAction = null;
                        yield return temp;
                    }
                    break;
                case Entity.EntityState.INTANGIBLE:
                    if (e.queuedAction is CrawlAction temp2)
                    {
                        e.queuedAction = null;
                        yield return temp2;
                    }
                    break;
                case Entity.EntityState.STUN:
                    // should always succeed.
                    yield return (CrawlAction)GD.Load<CSharpScript>("res://BaseGame/Actions/StunRecoveryAction.cs").New();
                    yield break;
                case Entity.EntityState.KNOCKDOWN:
                    // should always succeed.
                    yield return (CrawlAction)GD.Load<CSharpScript>("res://BaseGame/Actions/KnockdownWakeupAction.cs").New();
                    yield break;
                default:
                    break;
            }

            if (!e.isPlayer)
            {
                // naively confirm all actions.
                if (e.needsConfirmAction != null) { yield return e.needsConfirmAction; }

                foreach ((CrawlAction a, bool ignoreWarning) in e.species?.ai?.GetMoves(this, e))
                {
                    if (ignoreWarning || !a.GetWarnings(this, e).Any())
                    {
                        yield return a;
                    }
                }
            }
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
}
