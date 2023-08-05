using Godot;
using Godot.Collections;
using System;

namespace LizardState.Engine
{
    // TODO:? Extract Superclass with Entity.
    public class FloorItem : Resource
    {
        [Export] public int id;

        [Export] public int positionX;
        [Export] public int positionY;
        public AbsolutePosition position
        {
            get { return new AbsolutePosition(positionX, positionY); }
            set { positionX = value.x; positionY = value.y; }
        }

        [Export] public InventoryItem inventoryItem;

        public FloorItem() { }
    }
}
