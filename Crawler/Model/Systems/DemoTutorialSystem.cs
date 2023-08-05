using System;
using Godot;
using Godot.Collections;
using LizardState.Engine;

public class DemoTutorialSystem : Resource, CrawlerSystem
{
    [Export] bool movePrompt = false;
    [Export] bool abilitiesPrompt = false;
    [Export] int kills = 0;

    public void ProcessEvent(Model model, Dictionary ev)
    {
        if ((string)ev["action"] == "Move" && !movePrompt)
        {
            movePrompt = true;
            model.CoolerApiEvent(-1, "Print", "Use the numpad to move. Hold Ctrl to move 5 spaces.");
            model.CoolerApiEvent(-1, "Print", "Or, click to path somewhere. (Use arrow keys if you must.)");
        }
        if ((string)ev["action"] == "Downed")
        {
            Entity subject = model.GetEntity((int)ev["subject"]);
            if (subject.team != 0 && !abilitiesPrompt)
            {
                abilitiesPrompt = true;
                model.CoolerApiEvent(-1, "Print", "Press A for [A]bilities!");
            }
            if (subject.id == 0)
            {
                DumpStats(model);
            }
        }
        if ((string)ev["action"] == "Exit")
        {
            DumpStats(model);
        }
    }

    private void DumpStats(Model model)
    {
        DemoStatisticsSystem stats = model.GetSystem<DemoStatisticsSystem>();
        model.CoolerApiEvent(-1, "Print", "");
        if (stats.gotMoss)
        {
            model.CoolerApiEvent(-1, "Print", $"You got the moss and exited! (you win!)");
        }
        else
        {
            model.CoolerApiEvent(-1, "Print", $"You did not get the moss. (you win but less so)");
        }
        model.CoolerApiEvent(-1, "Print", "");
        model.CoolerApiEvent(-1, "Print", $"You spent {model.time} time units in the cave.");
        model.CoolerApiEvent(-1, "Print", $"You got {stats.kills} kills.");
        model.CoolerApiEvent(-1, "Print", $"You did {stats.playerDamage} damage.");
        model.CoolerApiEvent(-1, "Print", $"Your partner did {stats.partnerDamage}." + (stats.partnerDead ? " (rip)" : ""));
        if (stats.usedViKeys)
        {
            model.CoolerApiEvent(-1, "Print", $"You used Vi Keys! Cool!!!");
        }
        if (stats.usedMouse)
        {
            model.CoolerApiEvent(-1, "Print", "You used the mouse! (or you clicked somewhere. I can't tell.)");
        }
        if (stats.usedArrow)
        {
            model.CoolerApiEvent(-1, "Print", "You used/tried to use arrow keys! Try using a numpad or mouse next time!");
        }
        if (stats.usedNumpad)
        {
            model.CoolerApiEvent(-1, "Print", "You used the numpad! You've got a big keyboard!");
        }
        model.CoolerApiEvent(-1, "Print", "");
        model.CoolerApiEvent(-1, "Print", "Thanks for playing!");
        model.CoolerApiEvent(-1, "Print", "Take a screenshot of this and send it to me!");
        // model.CoolerApiEvent(-1, "Print", "(Press F11 to restart! RNG will be different tho.)");
        model.CoolerApiEvent(-1, "Print", "");
        model.CoolerApiEvent(-1, "Print", "            -ArtlessAvian (Ryan)");
    }

    public void Run(Model model)
    {
        throw new System.NotImplementedException();
    }
}