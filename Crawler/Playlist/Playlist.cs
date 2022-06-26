using Godot;
using Godot.Collections;

/// <summary>
/// A sequence of floors. Defined by a sequence of generators.
/// References to old and current models are saved here too.
/// Generators can then access them.
/// </summary>
public class Playlist : Resource
{
    [Export] Array<LevelGenerator> generators;
    [Export] Array<Model> previousModels;
    [Export] int current = -1;

    // [Export] Entity[] _playerTeam; // = {GD.Load<>};

    public Model FirstModel()
    {
        if (current != -1) { return null; }
        current = 0;

        // generate the players
        // or just load them in. they're resources.
        // maybe theres some indirection, where it loads it from the meta-game's data.
        // in that case, whatever.

        return GenerateModel();
    }

    public Model NextModel(Model previous)
    {
        if (current >= generators.Count - 1) { return null; }
        if (previous is null) { return null; }

        previousModels.Add(previous);
        current += 1;

        // we still hold a reference to the players.
        // if we decide thats kind of wack, we can just yoink them from the previous model we just got.
        // also heal them, have them "eat", etc.

        return GenerateModel();
    }

    private Model GenerateModel()
    {
        Model model = GD.Load<PackedScene>("res://Crawler/Model/Model.tscn").Instance<Model>();
        model.playlist = this;

        return model;
    }
}