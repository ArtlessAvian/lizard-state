using Godot;
using System;
using System.Collections.Generic;
using LizardState.Engine;

namespace LizardState.Engine
{
    public class StunRecoveryAction : CrawlAction
    {
        public static StunRecoveryAction New()
        {
            return (StunRecoveryAction)GD.Load<CSharpScript>("res://Engine/Actions/StunRecoveryAction.cs").New();
        }

        public override bool Do(Model model, Entity e)
        {
            if (!IsValid(model, e))
            {
                return false;
            }

            model.CoolerApiEvent(e.id, "Unstun");
            e.state = Entity.EntityState.OK;
            return true;
        }

        public override bool IsValid(Model model, Entity e)
        {
            return true;
        }
    }
}
