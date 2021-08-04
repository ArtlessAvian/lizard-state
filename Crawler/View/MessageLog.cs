using Godot;
using Godot.Collections;

public class MessageLog : RichTextLabel
{
    // move responsibility to event handlers.
    public void HandleModelEvent(Dictionary ev, Array<Actor> roles)
    {
        string action = (string)ev["action"];
        if (action == "Swap")
        {
            Actor subject = roles[(int)ev["subject"]];            
            Actor obj = roles[(int)ev["object"]];            
            this.AppendBbcode($"\n * {subject.displayName} swaps with {obj.displayName}.");
        }
        else if (action == "Hit")
        {
            Actor subject = roles[(int)ev["subject"]];            
            Actor obj = roles[(int)ev["object"]];
            this.AppendBbcode($"\n [color=#aaaaaa]* {subject.displayName} hits the {obj.displayName}.[/color]");
            
            if ((bool)ev["stuns"])
            {
                this.AppendBbcode($"\n * {obj.displayName} is stunned!");
            }
        }
        else if (action == "Miss")
        {
            Actor subject = roles[(int)ev["subject"]];            
            Actor obj = roles[(int)ev["object"]];            
            this.AppendBbcode($"\n * {subject.displayName} misses the {obj.displayName}.");
        }
        else if (action == "Downed")
        {
            Actor subject = roles[(int)ev["subject"]];            
            this.AppendBbcode($"\n * {subject.displayName} is downed!");
        }
        else if (action == "Print")
        {
            string message = (string)ev["args"];
            this.AppendBbcode($"\n * {message}");
        }
    }
}
