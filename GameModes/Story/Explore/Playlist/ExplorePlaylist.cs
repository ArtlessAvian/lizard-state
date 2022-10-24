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
    [Export] int current = 0;
    [Export] Model currentModel = null;

    public void Reset()
    {
        current = 0;
        currentModel = null;
    }

    // [Export] Entity[] _playerTeam; // = {GD.Load<>};

    public Model GetCurrentModel()
    {
        if (currentModel == null)
        {
            // construct the players.
            Species playerSpecies = GD.Load<Resource>("res://Crawler/Model/Species/PlayerTegu.tres") as Species;
            Species partnerSpecies = GD.Load<Resource>("res://Crawler/Model/Species/PartnerAxolotl.tres") as Species;

            Entity player = (Entity)GD.Load<CSharpScript>("res://Crawler/Model/Entity.cs").New();
            Entity partner = (Entity)GD.Load<CSharpScript>("res://Crawler/Model/Entity.cs").New();

            player.SetSpecies(playerSpecies);
            partner.SetSpecies(partnerSpecies);

            player.SetTeam(0);
            partner.SetTeam(0);

            player.isPlayer = true;

            currentModel = GenerateModel(0, new Entity[] { player, partner });
        }

        return currentModel;
    }

    // Dunno why to pass in the previous.
    public Model CreateNextModel(Model previous)
    {
        if (current >= generators.Count - 1) { return null; }
        if (previous is null) { return null; }
        current += 1;

        Entity player = (Entity)previous.GetEntity(0).Duplicate();
        Entity partner = (Entity)previous.GetEntity(1).Duplicate();

        // have players "eat", heal them, etc.
        // eating logic here
        // heal/buff logic here.
        player.health = Mathf.Min(player.health + 5, player.species.maxHealth);
        partner.health = Mathf.Min(partner.health + 5, partner.species.maxHealth);

        currentModel = GenerateModel(current, new Entity[] { player, partner });

        return currentModel;
    }

    private Model GenerateModel(int index, Entity[] playerTeam)
    {
        Model model = GD.Load<CSharpScript>("res://Crawler/Model/Model.cs").New() as Model;
        generators[index].Generate(model, playerTeam);
        return model;
    }
}