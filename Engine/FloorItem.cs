using Godot;
using Godot.Collections;
using System;
using System.CodeDom;

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

        public static FloorItem New(AbsolutePosition position, InventoryItem item)
        {
            var instance = (FloorItem)GD.Load<CSharpScript>("res://Engine/FloorItem.cs").New();
            instance.position = position;
            instance.inventoryItem = item;
            return instance;
        }

        private FloorItem() { }
    }
}
