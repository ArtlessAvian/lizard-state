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
    [Export] bool skippedEating = false;

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
            // Species playerSpecies = GD.Load<Resource>("res://Crawler/Model/Species/PartnerGator.tres") as Species;
            Species partnerSpecies = GD.Load<Resource>("res://Crawler/Model/Species/PartnerAxolotl.tres") as Species;

            // position will edited.
            Entity player = playerSpecies.BuildEntity(new AbsolutePosition(-100, -100), 0);
            Entity partner = partnerSpecies.BuildEntity(new AbsolutePosition(-100, -100), 0);

            player.isPlayer = true;
            player.inventory.Add(GD.Load<ItemData>("res://Crawler/Model/ItemData/Something.tres").BuildInventoryItem());

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

        player.nextMove = 0;
        partner.nextMove = 0;
        player.state = Entity.EntityState.OK;
        partner.state = Entity.EntityState.OK;

        // have players "eat", heal them, etc.
        // if (!player.hasEaten)
        // {
        //     // eating logic here
        // }

        if (player.hasEaten)
        {
            player.hasEaten = false;
            skippedEating = false;
            // heal/buff logic here.
            player.health = Mathf.Min(player.health + 5, player.species.maxHealth);
            partner.health = Mathf.Min(partner.health + 5, partner.species.maxHealth);
        }
        else if (!skippedEating)
        {
            skippedEating = true;
            // player should be able to get 1 food for next cave.
            // if they're really stuck, then, oh no.
            player.health = 5;
            partner.health = 5;
        }
        else
        {
            // oh no.
            // abort mission?
        }

        currentModel = GenerateModel(current, new Entity[] { player, partner });

        return currentModel;
    }

    private Model GenerateModel(int index, Entity[] playerTeam)
    {
        Model model = GD.Load<CSharpScript>("res://Crawler/Model/Model.cs").New() as Model;
        generators[index].Generate(model, playerTeam);
        model.ResourceName = $"{index} of {this.ResourceName}";
        return model;
    }
}