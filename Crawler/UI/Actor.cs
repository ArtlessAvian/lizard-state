using Godot;
using System;

// Like a ViewModel. Also, a pile of callbacks for the View.
public partial class Actor : Sprite
{
    (int x, int y) targetPosition;

    public void SyncWithEntity(Entity subject)
    {
        targetPosition = subject.position;
        Position = new Vector2(
            targetPosition.x * Crawler.TILESIZE.x,
            targetPosition.y * Crawler.TILESIZE.y
        );
    }

    public void Perform(string action, object args)
    {
        // EmitSignal("Action", action);
        // EmitSignal(action, args);

        if (action == "Moved")
        {
            (int x, int y) cast = ((int x, int y))args;

            int dx = cast.x - targetPosition.x;
            int dy = cast.y - targetPosition.y;

            targetPosition.x = cast.x;
            targetPosition.y = cast.y;
    
            this.FaceDirection(dx, dy);        
        }
    }

    private void FaceDirection(int dx, int dy)
    {
        Rect2 rect = this.RegionRect;
        if (dx != 0)
        {
            if (dx > 0)
            {
                this.RegionRect = new Rect2(rect.Size.x * 1, 0, rect.Size);
            }
            else
            {
                this.RegionRect = new Rect2(rect.Size.x * 3, 0, rect.Size);
            }
        }
        else
        {
            if (dy >= 0)
            {
                this.RegionRect = new Rect2(rect.Size.x * 0, 0, rect.Size);
            }
            else
            {
                this.RegionRect = new Rect2(rect.Size.x * 2, 0, rect.Size);
            }
        }
    }

    public override void _Process(float delta)
    {
        Position = Position.LinearInterpolate(
            new Vector2(
                targetPosition.x * Crawler.TILESIZE.x,
                targetPosition.y * Crawler.TILESIZE.y
            ),
            1 - Mathf.Pow(1-0.3f, delta * 60f)
        );
    }
}
