using Godot;
using System;
using System.Collections.Generic;

public partial class Crawler : Node2D
{
    public static (int x, int y) TILESIZE = (32, 24);

    // not saved
    public List<ModelEvent> eventQueue;
    Dictionary<Entity, Actor> roles;

    // saved, of course.
    public Model model;
    public bool notPlayerTurn = false;

    // convenience
    public Actor playerActor;

    Crawler()
    {
        eventQueue = new List<ModelEvent>();
        roles = new Dictionary<Entity, Actor>();
    }

    public override void _Ready()
    {
        if (model is null)
        {
            EditorGenerator gen = new EditorGenerator("res://Crawler/Maps/BigTest.tscn");
            model = gen.Generate(eventQueue);
        }
        this.ClearQueue();
        
        playerActor = roles[model.GetPlayer()];
        GetNode<CrawlerCamera>("Camera2D").focus = playerActor;

        GetNode("Map").Set("tile_data", model.map.Get("tile_data"));
    }

    public override void _Process(float delta)
    {
        while (notPlayerTurn) // and not timed out
        {
            if (!model.DoEntityAction(eventQueue))
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
            if (ev.subject is null)
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
                    Actor puppet = GD.Load<PackedScene>($"res://Crawler/View/Actors/{ev.subject.species.ResourceName}.tscn").Instance() as Actor;
                    roles.Add(ev.subject, puppet);
                    puppet.SyncWithEntity(ev.subject);
                    GetNode("Actors").AddChild(puppet);
                }
                else
                {
                    // Delegate command to Actor
                    roles[ev.subject].PerformAsSubject(ev, roles);
                    if (ev.obj is Entity obj)
                    {
                        roles[obj].PerformAsObject(ev, roles);
                    }
                }
            }

            eventQueue.RemoveAt(0);

            if (eventQueue.Count == 0)
            {
                GetNode<RichTextLabel>("UILayer/Time").Text = $"Time: {model.time}";
            }
        }
    }

    private bool AnyActorAnimating()
    {
        foreach (Actor a in roles.Values)
        {
            if (a.IsAnimating())
            {
                return true;
            }
        }
        return false;
    }
}
