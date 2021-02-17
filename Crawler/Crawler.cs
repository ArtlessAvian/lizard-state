using Godot;
using System;
using System.Collections.Generic;

public partial class Crawler : Node2D
{
    public static (int x, int y) TILESIZE = (32, 24);

    public List<ModelEvent> eventQueue;
    List<Actor> roles;

    public Model model;
    public bool notPlayerTurn = false;

    // convenience
    public Actor playerActor;

    public bool impatientMode = false; // super buggy but convenient

    Crawler()
    {
        eventQueue = new List<ModelEvent>();
        roles = new List<Actor>();
    }

    public override void _Ready()
    {
        // Failsafe.
        if (model is null)
        {
            EditorGenerator gen = new EditorGenerator("res://Crawler/Maps/BigTest.tscn");
            model = gen.Generate(eventQueue);
        }
        this.ClearQueue();
        
        playerActor = roles[0];
        GetNode<CrawlerCamera>("Camera2D").focus = playerActor;

        // Temporary hacks!!
        // GetNode("Map").Set("tile_data", model.map.map.Get("tile_data"));
        // model.map.visibility.CellSize = new Vector2(40, 40); 
        // model.map.visibility.Scale = new Vector2(0.8f, 0.6f);
        // model.map.visibility.Position = new Vector2(-16, -12);
        // model.map.visibility.TileSet = GD.Load<TileSet>("res://Crawler/View/Assets/Visibility.tres");
        // AddChild(model.map.visibility);
    }

    public override void _Process(float delta)
    {
        uint start = OS.GetTicksMsec();
        this.FillQueue(start);
        this.ClearQueue();
    }

    private void FillQueue(uint start)
    {
        while (notPlayerTurn) // and not timed out
        {
            if (!model.DoEntityAction())
            {
                // let the player move again.
                notPlayerTurn = false;
                break;
            }

            if (OS.GetTicksMsec() - start > 1000/240f)
            {
                GD.PrintErr("Timed out!");
            }
        }
    }

    private void ClearQueue()
    {
        if (eventQueue.Count == 0) {return;}

        while (eventQueue.Count > 0)
        {
            ModelEvent ev = eventQueue[0];
            if (ev.subject == -1 && ev.action == "Wait")
            {
                if (!impatientMode && AnyActorAnimating()) { break; }
            }
            eventQueue.RemoveAt(0);

            HandleNonActorEvent(ev);

            if (ev.subject >= 0)
            {
                roles[ev.subject].PerformAsSubject(ev, roles);
                if (ev.obj >= 0) { roles[ev.obj].PerformAsObject(ev, roles); }
            }

            GD.PrintS(ev.subject, ev.action, ev.args, ev.obj);
            GetNode<RichTextLabel>("UILayer/DebugLog").AppendBbcode("\n * " + ev.subject + " " + ev.action + " " + ev.args + " " + ev.obj);
        }

        // Runs after loop, if the queue wasn't already empty!
        GetNode<RichTextLabel>("UILayer/Time").Text = $"(Debug) Time: {model.time}";
        // foreach (Entity e in model.entities)
        // {
        //     roles[e].SyncWithEntity(e);
        // }
    }

    private void HandleNonActorEvent(ModelEvent ev)
    {
        if (ev.action == "Create")
        {
            // This entity info should never be abused outside this section!!
            Entity entity = ev.args as Entity;
            Actor puppet = GD.Load<PackedScene>($"res://Crawler/View/Actors/{entity.species.ResourceName}.tscn").Instance() as Actor;
            roles.Add(puppet);
            puppet.SyncWithEntity(entity);
            GetNode("Map/VisibleWalls/Actors").AddChild(puppet);
        }

        else if (ev.action == "SeeMap")
        {
            ((int x, int y) center, int[,] tiles) = (((int, int), int[,]))ev.args;
            GetNode<MapView>("Map").AddVision(ev.subject, center, tiles);
        }

        else if (ev.action == "Print")
        {
            string message = (string)ev.args;
            GD.Print(message);
            GetNode<RichTextLabel>("UILayer/MessageLog").AppendBbcode("\n * " + message);
        }
    }

    private bool AnyActorAnimating()
    {
        foreach (Actor a in roles)
        {
            if (a.IsAnimating())
            {
                return true;
            }
        }
        return false;
    }
}
