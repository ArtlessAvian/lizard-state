using Godot;
using Godot.Collections;
using System.Collections.Generic;

namespace LizardState.Engine
{
    /// <summary>
    /// Stores map information.
    /// </summary>
    // Parasitically based on TileMap.
    // Would change, but its quite convenient.
    [Tool]
    public class CrawlerMap : Resource
    {
        [Export]
        public SparseMatrix tiles;

        // Creates an empty map. It's "valid" but very boring.
        // Prefer the constructor with params.
        public CrawlerMap()
        {
            tiles = (SparseMatrix)GD.Load<CSharpScript>("res://Engine/SparseMatrix.cs").New();
        }

        public CrawlerMap(SparseMatrix tiles)
        {
            this.tiles = tiles;
        }

        // public new int GetCell(int x, int y)
        // {
        //     return base.GetCell(x/2, y/2);
        // }

        public bool TileIsWall(AbsolutePosition position)
        {
            return TileIsWall(tiles.GetCell(position.x, position.y));
        }

        public static bool TileIsWall(int id)
        {
            return id == -1 || id == 6;
        }
    }
}
