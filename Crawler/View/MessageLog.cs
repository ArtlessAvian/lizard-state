using Godot;
using System;
using System.Collections.Generic;

public class MessageLog : RichTextLabel
{
    public void HandleModelEvent(ModelEvent ev, List<Actor> roles)
    {
        if (ev.action == "Swap")
        {
            Actor subject = roles[ev.subject];            
            Actor obj = roles[ev.obj];            
            this.AppendBbcode($"\n * {subject.displayName} swaps with {obj.displayName}.");
        }
        else if (ev.action == "Hit")
        {
            AttackResult result = (AttackResult)ev.args; 
            Actor subject = roles[ev.subject];
            Actor obj = roles[ev.obj];
            this.AppendBbcode($"\n * {subject.displayName} hits the {obj.displayName}.");
            
            if (result.stuns)
            {
                this.AppendBbcode($"\n * {obj.displayName} is stunned!");
            }
        }
        else if (ev.action == "Miss")
        {
            Actor subject = roles[ev.subject];
            Actor obj = roles[ev.obj];
            this.AppendBbcode($"\n * {subject.displayName} misses the {obj.displayName}.");
        }
        else if (ev.action == "Downed")
        {
            Actor subject = roles[ev.subject];
            this.AppendBbcode($"\n * {subject.displayName} is downed!");
        }
        else if (ev.action == "Print")
        {
            string message = (string)ev.args;
            this.AppendBbcode($"\n * {message}");
        }
    }
}
