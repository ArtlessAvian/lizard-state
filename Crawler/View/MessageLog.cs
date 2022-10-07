using System;
using Godot;
using Godot.Collections;

public class MessageLog : RichTextLabel
{
    string previousMessage = "";
    int repetitions = 0;

    public void AddMessage(string message)
    {
        if (message == previousMessage)
        {
            repetitions++;
            this.AppendBbcode($" x{repetitions}");
        }
        else
        {
            previousMessage = message;
            repetitions = 1;
            AppendBbcode($"\n * {message}");
        }
    }
}
