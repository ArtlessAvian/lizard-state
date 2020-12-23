using Godot;
using Godot.Collections;
using System;

public interface CrawlerAI
{
    Action GetMove(ModelAPI api, Entity e);
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

    public Action GetMove(ModelAPI api, Entity e)
    {
        return ai.GetMove(api, e);
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

    public Action GetMove(ModelAPI api, Entity e)
    {
        Entity player = api.GetPlayer();
        int dx = player.position.x - e.position.x;
        int dy = player.position.y - e.position.y;

        if (Math.Abs(dx) <= 2 && Math.Abs(dy) <= 2)
        {
            return new MoveAction((0, 0));
        }

        dx = Math.Sign(dx);
        dy = Math.Sign(dy);

        return new MoveAction((dx, dy));
    }

    public Dictionary SaveToDict()
    {
        Dictionary dict = new Dictionary();
        dict["Type"] = "Partner";
        return dict;
    }
}