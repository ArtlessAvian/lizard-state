using Godot;
using Godot.Collections;
using System;

// TODO:? Extract Superclass with Entity.
public class FloorItem : Resource
{
    public int id;

    public int positionX;
    public int positionY;
    public (int x, int y) position
    {
        get { return (positionX, positionY); }
        set { positionX = value.x; positionY = value.y; }
    }

    

    public FloorItem() {}

    public FloorItem(Dictionary dict)
    {
        this.positionX = (int)dict["positionX"];
        this.positionY = (int)dict["positionY"];
    }

    public Dictionary SaveToDictionary()
    {
        Dictionary dict = new Dictionary();
        dict["positionX"] = positionX;
        dict["positionY"] = positionY;
        return dict;
    }
}
