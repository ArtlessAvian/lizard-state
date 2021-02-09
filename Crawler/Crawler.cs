using Godot;
using System;
using System.Collections.Generic;

public partial class Crawler : Node2D
{
    public static (int x, int y) TILESIZE = (32, 24);

    // not saved
    public List<ModelEvent> eventQueue;
    List<Actor> roles;

    // saved, of course.
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

        GetNode("Map").Set("tile_data", model.map.Get("tile_data"));
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
                // reset player actor
                // TODO: This doesn't work lmao, gotta queue it
                // playerActor.GetNode<AnimatedSprite>("AnimatedSprite").Frame = 0;
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
                if (ev.action == "Print")
                {
                    string message = (string)ev.args;
                    GD.Print(message);
                    GetNode<RichTextLabel>("UILayer/MessageLog").AppendBbcode("\n * " + message);
                }
            }
            else
            {
                if (ev.action == "Created")
                {
                    // This entity info should never be abused outside this section!!
                    Entity entity = ev.args as Entity;
                    Actor puppet = GD.Load<PackedScene>($"res://Crawler/View/Actors/{entity.species.ResourceName}.tscn").Instance() as Actor;
                    roles.Add(puppet);
                    puppet.SyncWithEntity(entity);
                    GetNode("Actors").AddChild(puppet);
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
            }

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
