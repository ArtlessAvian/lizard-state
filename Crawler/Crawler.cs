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
    bool notPlayerTurn = false;

    Crawler()
    {
        eventQueue = new List<ModelEvent>();
        roles = new Dictionary<Entity, Actor>();
    }

    public override void _Ready()
    {
        if (model is null)
        {
            EditorGenerator gen = new EditorGenerator("res://Crawler/Maps/Debuggy.tscn");
            model = gen.Generate(eventQueue);
        }

        GetNode("Map").Set("tile_data", model.map.Get("tile_data"));
    }

    public override void _Process(float delta)
    {
        if (Input.IsActionJustPressed("ui_cancel"))
        {
            GetNode("Map").Set("tile_data", model.map.Get("tile_data"));
        }

        while (notPlayerTurn) // and not timed out
        {
            if (!model.DoEntityAction(eventQueue))
            {
                // let the player move again.
                notPlayerTurn = false;
                break;
            }
        }

        while (eventQueue.Count > 0)
        {
            ModelEvent ev = eventQueue[0];
            eventQueue.RemoveAt(0);

            if (ev.action == "Created")
            {
                Actor puppet = GD.Load<PackedScene>($"res://Crawler/UI/Actors/{ev.subject.species.ResourceName}.tscn").Instance() as Actor;
                roles.Add(ev.subject, puppet);
                puppet.SyncWithEntity(ev.subject);
                GetNode("Actors").AddChild(puppet);
            }

            // GD.PrintS(ev.subject, ev.action, ev.args);

            roles[ev.subject].Perform(ev.action, ev.args);
        }
    }
}
