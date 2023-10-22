using Godot;
using GDArray = Godot.Collections.Array;
using System.Collections.Generic;
using System;
using System.Linq;

namespace LizardState.Engine
{
    // Resource friendly.
    // Drop in replacement for TileMap. API will change (hahahhahahh nice joke)
    // (TileMap is a node)
    public class SparseMatrix : Resource
    {
        private const int CELL_LEN_POW = 2; // magic number. extra space for dictionary vs wasted array space.
        private const int CELL_LEN = 1 << CELL_LEN_POW;
        private const int CELL_AREA = 1 << (2 * CELL_LEN_POW);

        [Export]
        private Dictionary<Vector2, int[]> chunks = new Dictionary<Vector2, int[]>();

        public static SparseMatrix New()
        {
            return (SparseMatrix)GD.Load<CSharpScript>("res://Engine/SparseMatrix.cs").New();
        }

        private SparseMatrix() { }

        private (Vector2, int) CellToChunkAndIndex(int x, int y)
        {
            // TODO: Check if negative bitshift is bad.
            Vector2 chunkId = new Vector2(x >> CELL_LEN_POW, y >> CELL_LEN_POW);
            int modx = (x % CELL_LEN + CELL_LEN) % CELL_LEN;
            int mody = (y % CELL_LEN + CELL_LEN) % CELL_LEN;
            return (chunkId, mody * CELL_LEN + modx);
        }

        private (Vector2, int) CellToChunkAndIndex(Vector2 vec)
        {
            return CellToChunkAndIndex((int)vec.x, (int)vec.y);
        }

        private Vector2 ChunkAndIndexToCell(Vector2 chunk, int index)
        {
            return new Vector2(
                chunk.x * CELL_LEN + index % CELL_LEN,
                chunk.y * CELL_LEN + (int)(index / CELL_LEN)
            );
        }

        public void ReadFromTilemap(TileMap map)
        {
            var usedChunks = new System.Collections.Generic.HashSet<Vector2>();
            foreach (Vector2 vec in map.GetUsedCells())
            {
                (Vector2 chunkId, int _) = CellToChunkAndIndex(vec);
                usedChunks.Add(chunkId);
            }
            foreach (Vector2 chunkId in usedChunks)
            {
                int[] chunk = new int[CELL_AREA];
                for (int i = 0; i < CELL_AREA; i++)
                {
                    Vector2 cell = ChunkAndIndexToCell(chunkId, i);
                    chunk[i] = map.GetCellv(cell);
                }
                chunks.Add(chunkId, chunk);
            }
        }

        public void DumpToTilemap(TileMap map)
        {
            foreach (Vector2 chunkId in chunks.Keys)
            {
                int[] chunk = chunks[chunkId];
                for (int i = 0; i < CELL_AREA; i++)
                {
                    Vector2 cell = ChunkAndIndexToCell(chunkId, i);
                    map.SetCellv(cell, chunk[i]);
                }
            }
        }

        // Acts as TileMap

        public void SetCell(int x, int y, int tile)
        {
            (Vector2 chunkId, int index) = CellToChunkAndIndex(x, y);
            if (!chunks.ContainsKey(chunkId))
            {
                int[] chunk = new int[CELL_AREA];
                for (int i = 0; i < CELL_AREA; i++) { chunk[i] = -1; }
                chunks.Add(chunkId, chunk);
            }

            int[] copy = chunks[chunkId];
            copy[index] = tile;
            chunks[chunkId] = copy;
        }

        public void SetCellv(Vector2 vec, int tile)
        {
            SetCell((int)vec.x, (int)vec.y, tile);
        }

        public void SetCellv(AbsolutePosition vec, int tile)
        {
            SetCell(vec.x, vec.y, tile);
        }

        public int GetCell(int x, int y)
        {
            // TODO: Check if negative bitshift is bad.
            (Vector2 chunkId, int index) = CellToChunkAndIndex(x, y);
            if (!chunks.ContainsKey(chunkId)) { return -1; }
            return chunks[chunkId][index];
        }

        public int GetCellv(Vector2 vec)
        {
            return GetCell((int)vec.x, (int)vec.y);
        }

        public int GetCellv(AbsolutePosition vec)
        {
            return GetCell(vec.x, vec.y);
        }

        [Obsolete("Prefer Iterator versions to avoid marshalling. (Unless calling from Godot)")]
        public GDArray GetUsedCells()
        {
            return new GDArray(GetUsedCellsIterator().Select(x => new Vector2(x.x, x.y)));
        }

        [Obsolete("Prefer Iterator versions to avoid marshalling. (Unless calling from Godot)")]
        public GDArray GetUsedCellsById(int id)
        {
            return new GDArray(GetUsedCellsByIdIterator(id).Select(x => new Vector2(x.x, x.y)));
        }

        public IEnumerable<AbsolutePosition> GetUsedCellsIterator()
        {
            foreach (Vector2 chunkId in chunks.Keys)
            {
                for (int i = 0; i < CELL_AREA; i++)
                {
                    if (chunks[chunkId][i] != -1)
                    {
                        Vector2 cell = ChunkAndIndexToCell(chunkId, i);
                        yield return new AbsolutePosition((int)cell.x, (int)cell.y);
                    }
                }
            }
        }

        public IEnumerable<AbsolutePosition> GetUsedCellsByIdIterator(int id)
        {
            foreach (Vector2 chunkId in chunks.Keys)
            {
                for (int i = 0; i < CELL_AREA; i++)
                {
                    if (chunks[chunkId][i] == id)
                    {
                        Vector2 cell = ChunkAndIndexToCell(chunkId, i);
                        yield return new AbsolutePosition((int)cell.x, (int)cell.y);
                    }
                }
            }
        }

        public void FindAndReplace(int find, int replace)
        {
            foreach (Vector2 chunkId in chunks.Keys)
            {
                for (int i = 0; i < CELL_AREA; i++)
                {
                    if (chunks[chunkId][i] == find)
                    {
                        chunks[chunkId][i] = replace;
                    }
                }
            }
        }
    }
}
