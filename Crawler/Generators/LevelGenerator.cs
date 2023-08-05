using Godot;
using Godot.Collections;
using System.Collections.Generic;

namespace LizardState.Engine
{
    /// <summary>
    /// Helps fill out an empty model.
    /// </summary>
    // TODO: Create a model from scratch.
    public abstract class LevelGenerator : Resource
    {
        public abstract Model Generate(Model model, Entity[] playerTeam);
        public abstract void GenerateMap(SparseMatrix tiles);
    }
}
