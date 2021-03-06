using System.Collections.Generic;
using Godot;
using Godot.Collections;

public class DemoStatisticsSystem : Node, CrawlerSystem
{
    public int kills = 0;
    public int playerDamage = 0;
    public int partnerDamage = 0;
    public bool partnerDead = false;
    public bool gotMoss = false;
    
    public bool usedMouse = false;
    public bool usedArrow = false;
    public bool usedNumpad = false;
    public bool usedViKeys = false;

    public void ProcessEvent(Model model, Dictionary ev)
    {
        if ((string)ev["action"] == "Downed")
        {
            Entity subject = model.GetEntity((int)ev["subject"]);
            if (subject.team != 0)
            {
                kills++;
            }
            else if (subject.id != 0)
            {
                partnerDead = true;
            }
        }

        if ((string)ev["action"] == "Hit")
        {
            Entity subject = model.GetEntity((int)ev["subject"]);
            if (subject.id == 0)
            {
                playerDamage += (int)(ev["hit"] as Dictionary)["damage"]; // ew
            }
            if (subject.id == 1)
            {
                partnerDamage += (int)(ev["hit"] as Dictionary)["damage"];
            }
        }

        if ((string)ev["action"] == "Print")
        {
            if ((string)ev["args"] == "Got the moss.")
            {
                gotMoss = true;
            }
        }
    }

    public void Run(Model model)
    {
        throw new System.NotImplementedException();
    }

    // These are probably all in numerical order or something.
    List<KeyList> arrows = new List<KeyList>(){KeyList.Left, KeyList.Right, KeyList.Up, KeyList.Down};
    List<KeyList> numpad = new List<KeyList>(){KeyList.Kp1, KeyList.Kp2, KeyList.Kp3, KeyList.Kp4, KeyList.Kp5, KeyList.Kp6, KeyList.Kp7, KeyList.Kp8, KeyList.Kp9};
    List<KeyList> viKeys = new List<KeyList>(){KeyList.H, KeyList.J, KeyList.K, KeyList.L, KeyList.B, KeyList.N, KeyList.Y, KeyList.U, KeyList.Period};

    // systems aren't supposed to do this, but,
    public override void _Input(InputEvent ev)
    {
        if (ev is InputEventMouseButton)
        {
            usedMouse = true;
        }
        if (ev is InputEventKey evKey)
        {
            if (arrows.Contains((KeyList)evKey.Scancode))
            {
                usedArrow = true;
            }
            else if (numpad.Contains((KeyList)evKey.Scancode))
            {
                usedNumpad = true;
            }
            else if (viKeys.Contains((KeyList)evKey.Scancode))
            {
                // wow gamer
                usedViKeys = true;
            }
        }
    }
}