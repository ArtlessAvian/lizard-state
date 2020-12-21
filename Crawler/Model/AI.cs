using Godot;
using Godot.Collections;
using System;

public interface CrawlerAI
{
    Action GetMove();
    Dictionary SaveToDict();
}

public class AI : CrawlerAI
{
    CrawlerAI ai;

    public AI(string type)
    {
        ai = new PartnerAI();
    }

    public AI(Dictionary dict)
    {
        ai = new PartnerAI(dict);
    }

    public Action GetMove()
    {
        return ai.GetMove();
    }

    public Dictionary SaveToDict()
    {
        return ai.SaveToDict();
    }
}

public class PartnerAI : CrawlerAI
{
    public PartnerAI()
    {

    }

    public PartnerAI(Dictionary dict)
    {

    }

    public Action GetMove()
    {
        return new MoveAction((0, 0));
    }

    public Dictionary SaveToDict()
    {
        Dictionary dict = new Dictionary();
        dict["Type"] = "Partner";
        return dict;
    }
}