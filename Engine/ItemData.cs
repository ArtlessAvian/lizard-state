using System;
using Godot;

namespace LizardState.Engine
{
    public class ItemData : Resource
    {
        [Export]
        public CrawlAction action; // duplicate me, don't use directly

        [Export]
        public int maxUses = 5;

        public ItemData()
        {
            if (ResourceName == "")
            {
                ResourceName = "Something";
            }
        }

        [Obsolete]
        public InventoryItem BuildInventoryItem()
        {
            return GD.Load<CSharpScript>("res://Engine/InventoryItem.cs").New(this) as InventoryItem;
        }
    }
}