using Godot;
using Godot.Collections;
using System;
using System.Collections.Generic;

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
        List<Entity> entities = api.GetEntities(e.position.x, e.position.y, 1);
        foreach (Entity other in entities)
        {
            if (other.team != e.team && !other.downed)
            {
                int dx = other.position.x - e.position.x;
                int dy = other.position.y - e.position.y;

                return new AttackAction((dx, dy));
            }
        }

        entities = api.GetEntities(e.position.x, e.position.y, 5);

        foreach (Entity other in entities)
        {
            if (other.team != e.team && !other.downed)
            {
                int dx = other.position.x - e.position.x;
                int dy = other.position.y - e.position.y;

                dx = Math.Sign(dx);
                dy = Math.Sign(dy);

                return new MoveAction((dx, dy));
            }
        }

        foreach (Entity other in entities)
        {
            if (other.team == e.team)
            {
                int dx = other.position.x - e.position.x;
                int dy = other.position.y - e.position.y;

                if (Math.Abs(dx) > 2 || Math.Abs(dy) > 2)
                {
                    dx = Math.Sign(dx);
                    dy = Math.Sign(dy);

                    return new MoveAction((dx, dy));
                }
            }
        }
        return new MoveAction((0, 0));
    }

    public Dictionary SaveToDict()
    {
        Dictionary dict = new Dictionary();
        dict["Type"] = "Partner";
        return dict;
    }
}