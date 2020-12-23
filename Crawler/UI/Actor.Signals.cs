using Godot;
using System;

public partial class Actor : Sprite
{
    [Signal]
    public delegate void Action(String action);
    [Signal]
    public delegate void Created(String args);
    [Signal]
    public delegate void Damaged(String args);
}
