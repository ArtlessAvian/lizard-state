using Godot;
using System;
using System.Collections.Generic;

public partial class Crawler : Node2D
{
    public static (int x, int y) TILESIZE = (32, 24);

    public List<ModelEvent> eventQueue;
    List<Actor> roles;
    Dictionary<int, ((int, int), int[,])> vision;

    public Model model;
    public bool notPlayerTurn = false;

    // convenience
    public Actor playerActor;

    Crawler()
    {
        eventQueue = new List<ModelEvent>();
        roles = new List<Actor>();
    }

    public override void _Ready()
    {
        if (model is null)
        {
            EditorGenerator gen = new EditorGenerator("res://Crawler/Maps/BigTest.tscn");
            model = gen.Generate(eventQueue);
        }
        this.ClearQueue();
        
        playerActor = roles[0];
        GetNode<CrawlerCamera>("Camera2D").focus = playerActor;

        GetNode<TileMap>("Map").Clear();

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
        while (notPlayerTurn) // and not timed out
        {
            // if (eventQueue.Count > 0 && eventQueue[eventQueue.Count - 1].action == "Wait")
            // {
            //     break;
            // }
            if (!model.DoEntityAction())
            {
                // let the player move again.
                notPlayerTurn = false;
                break;
            }
        }
        this.ClearQueue();
    }

    private void ClearQueue()
    {
        while (eventQueue.Count > 0)
        {
            ModelEvent ev = eventQueue[0];
            if (ev.subject == -1)
            {
                if (ev.action == "Wait")
                {
                    if (AnyActorAnimating()) { break; }
                }
                else
                {
                    HandleSpecialModelEvent(ev);
                }
            }
            else
            {
                // Delegate command to Actor
                roles[ev.subject].PerformAsSubject(ev, roles);
                if (ev.obj != -1)
                {
                    roles[ev.obj].PerformAsObject(ev, roles);
                }
            }

            GD.PrintS(ev.subject, ev.action, ev.args, ev.obj);
            eventQueue.RemoveAt(0);

            if (eventQueue.Count == 0)
            {
                GetNode<RichTextLabel>("UILayer/Time").Text = $"Time: {model.time}";
                // foreach (Entity e in model.entities)
                // {
                //     roles[e].SyncWithEntity(e);
                // }
            }
        }
    }

    private void HandleSpecialModelEvent(ModelEvent ev)
    {
        if (ev.action == "Create")
        {
            // This entity info should never be abused outside this section!!
            Entity entity = ev.args as Entity;
            Actor puppet = GD.Load<PackedScene>($"res://Crawler/View/Actors/{entity.species.ResourceName}.tscn").Instance() as Actor;
            roles.Add(puppet);
            puppet.SyncWithEntity(entity);
            GetNode("Actors").AddChild(puppet);
        }

        else if (ev.action == "UpdateVision")
        {

            ((int x, int y) center, int[,] tiles) = (((int, int), int[,]))ev.args;
            int r = tiles.GetLength(0) / 2; // floored
            TileMap map = GetNode<TileMap>("Map");
            TileMap visibility = GetNode<TileMap>("Visibility");

            for (int dy = -r; dy <= r; dy++)
            {
                for (int dx = -r; dx <= r; dx++)
                {
                    int tile = tiles[dx + r, dy + r];
                    if (tile != -1) {
                        map.SetCell(center.x + dx, center.y + dy, tile);
                        visibility.SetCell(center.x + dx, center.y + dy, 2);
                    }
                }
            }
            
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
