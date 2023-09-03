using Godot;
using Godot.Collections;

namespace LizardState.Engine
{
    public abstract class CrawlerSystem : Resource
    {
        public abstract void ProcessEvent(Model model, Dictionary @ev);
        public abstract void Run(Model model);
    }
}