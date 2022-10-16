using Godot;
using Godot.Collections;

/// <summary>
/// A sequence of floors. Defined by a sequence of generators.
/// References to old and current models are saved here too.
/// Generators can then access them.
/// </summary>
public class ExplorePlaylist : Resource
{
    [Export] Array<LevelGenerator> generators;
    // [Export] Array<Model> previousModels;
    [Export] int current = 0;
    [Export] Model currentModel = null;

    // [Export] Entity[] _playerTeam; // = {GD.Load<>};

    public Model GetCurrentModel()
    {
        if (currentModel == null)
        {
            currentModel = GenerateModel(0);
        }

        // generate the players
        // or just load them in. they're resources.
        // maybe theres some indirection, where it loads it from the meta-game's data.
        // in that case, whatever.

        return currentModel;
    }

    public Model CreateNextModel(Model previous)
    {
        if (current >= generators.Count - 1) { return null; }
        if (previous is null) { return null; }

        // previousModels.Add(previous);
        current += 1;

        // we still hold a reference to the players.
        // if we decide thats kind of wack, we can just yoink or duplicate them from the previous model we just got.
        // also heal them, have them "eat", etc.

        return GenerateModel(current);
    }

    private Model GenerateModel(int index)
    {
        Model model = GD.Load<CSharpScript>("res://Crawler/Model/Model.cs").New() as Model;
        generators[index].Generate(model);
        return model;
    }
}