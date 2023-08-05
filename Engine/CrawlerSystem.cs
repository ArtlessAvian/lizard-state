using Godot.Collections;

namespace LizardState.Engine
{
    public interface CrawlerSystem
    {
        void ProcessEvent(Model model, Dictionary @ev);
        void Run(Model model);
    }
}