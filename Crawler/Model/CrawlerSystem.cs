using Godot.Collections;

public interface CrawlerSystem
{
    void ProcessEvent(Model model, Dictionary @ev);
    void Run(Model model);
}