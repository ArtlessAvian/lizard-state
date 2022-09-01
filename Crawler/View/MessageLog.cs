using Godot;
using Godot.Collections;

public class MessageLog : RichTextLabel
{
    string previousMessage = "";
    // int previousMessageStart = 0;
    int repetitions = 0;

    public void AddMessage(string message)
    {
        if (message == previousMessage)
        {
            repetitions++;
            // this.
            // this.BbcodeText = this.BbcodeText.Substr(0, previousMessageStart);
            this.AppendBbcode($" x{repetitions}");
        }
        else
        {
            // previousMessageStart = this.BbcodeText.Length;
            previousMessage = message;
            repetitions = 1;
            AppendBbcode(message);
        }
    }

    // move responsibility to event handlers.
    public void HandleModelEvent(Dictionary ev, Dictionary<int, Actor> roles)
    {
        string action = (string)ev["action"];
        if (action == "Swap")
        {
            Actor subject = roles[(int)ev["subject"]];
            Actor obj = roles[(int)ev["object"]];
            this.AddMessage($"\n * {subject.displayName} swaps with {obj.displayName}.");
        }
        else if (action == "AttackStartup")
        {
            Actor subject = roles[(int)ev["subject"]];
            this.AddMessage($"\n [color=#aaaaaa]* {subject.displayName} prepares to attack.[/color]");
        }
        else if (action == "AttackActive")
        {
            Actor subject = roles[(int)ev["subject"]];
            this.AddMessage($"\n * {subject.displayName} attacks!");
        }
        else if (action == "Hit")
        {
            Actor subject = roles[(int)ev["subject"]];
            Actor obj = roles[(int)ev["object"]];
            this.AddMessage($"\n * Stuns the {obj.displayName}!!");
        }
        else if (action == "Unstun")
        {
            Actor subject = roles[(int)ev["subject"]];
            this.AddMessage($"\n [color=#aaaaaa]* {subject.displayName} recovers.[/color]");
        }
        else if (action == "Rush")
        {
            Actor subject = roles[(int)ev["subject"]];
            Actor obj = roles[(int)ev["object"]];
            this.AddMessage($"\n [color=#ffaaaa]* {subject.displayName} hits the {obj.displayName}.[/color]");
        }
        else if (action == "Miss")
        {
            Actor subject = roles[(int)ev["subject"]];
            Actor obj = roles[(int)ev["object"]];
            this.AddMessage($"\n * {subject.displayName} misses the {obj.displayName}.");
        }
        else if (action == "Downed")
        {
            Actor subject = roles[(int)ev["subject"]];
            this.AddMessage($"\n * {subject.displayName} is downed!");
        }
        else if (action == "See")
        {
            Actor subject = roles[(int)ev["subject"]];
            Actor objectt = roles[(int)ev["object"]];
            this.AddMessage($"\n * {subject.displayName} spotted a {objectt.displayName}!");
        }
        else if (action == "Print")
        {
            string message = (string)ev["args"];
            this.AddMessage($"\n * {message}");
        }
        else if (action == "Debug")
        {
            string message = (string)ev["args"];
            this.AddMessage($"\n * [color=#00ffff]{message}[/color]");
        }
    }
}
