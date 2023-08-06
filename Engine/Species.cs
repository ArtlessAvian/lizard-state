using Godot;
using System;
using System.Collections.Generic;

namespace LizardState.Engine
{
    // TODO: Replace with prototype pattern
    public class Species : Resource
    {
        // Game Logic
        [Export] public AI ai = null;

        [Export] public CrawlAction bumpAttack;
        [Export] public List<CrawlAction> abilities;

        // Stat Block
        [Export] public int maxHealth = 30;

        // View stuff
        [Export] public string displayName;

        // TODO: Obsolete. Prefer initializer syntax?
        public Entity BuildEntity(AbsolutePosition position, int team)
        {
            Entity entity = (Entity)GD.Load<CSharpScript>("res://Engine/Entity.cs").New();
            entity.SetSpecies(this);
            entity.SetTeam(team);
            entity.position = position;
            return entity;
        }
    }
}