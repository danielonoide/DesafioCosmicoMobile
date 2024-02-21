using Godot;
using System;
using System.Collections.Generic;

public abstract class ProjectileLauncher : Area2D
{

    public static bool selected = false;
    protected float lineWidth=10;
    protected Vector2 direction = Vector2.Zero;
    protected Vector2 initialVelocity = Vector2.Zero;
    protected float speed=0;
    protected List<Node> collidingBodies = new();
    protected Area2D collisionArea;
    protected Line2D line;
    protected RectangleShape2D collisionShape;
    protected AudioStreamPlayer restartSound;

	protected float degAngle=0;

    protected abstract Vector2 StartingPoint{get; }

    protected abstract void RestartLaunch();

    protected abstract void CalculateInitialVelocity();

    public override void _Ready()
    {
        line = GetNode<Line2D>("Line2D");
        line.Width = lineWidth;

        restartSound = GetNode<AudioStreamPlayer>("LaunchRestartSound");
        collisionArea = GetNode<Area2D>("Colisionador");
        collisionShape = new RectangleShape2D
        {
            Extents = new Vector2(1, lineWidth / 2)
        };
    }

    public override void _PhysicsProcess(float delta)
    {
        if (!selected) return;

        UpdateTrajectory(delta);
    }

    protected void RemoveCollisions()
    {
        foreach (CollisionShape2D collision in collisionArea.GetChildren())
        {
            collision.QueueFree();
        }
    }

    protected void CreateCollisionShape(Vector2 position, Vector2 extents, float angle)
    {
        CollisionShape2D collision = new()
        {
            Position = position,
            Rotation = angle
        };
        RectangleShape2D rectangleShape2D = new()
        {
            Extents = extents
        };
        collision.Shape = rectangleShape2D;
		
        collisionArea.AddChild(collision);
    }

    protected void UpdateTrajectory(float delta)
    {
        line.ClearPoints();
        line.AddPoint(Vector2.Zero);

        CalculateInitialVelocity();

        Vector2 velocity=initialVelocity;
        Vector2 newPos = StartingPoint; //starting point

        int pointsNumber=0;
        switch(Globals.Gravity)
        {
            case (int)Constants.Gravities.MarsGravity:
                pointsNumber=300;
                break;

            case (int)Constants.Gravities.MoonGravity:
                pointsNumber=500;
                break;

            case (int)Constants.Gravities.SpaceGravity:
                pointsNumber=600;
                break;   
        }

        for (int i = 0; i < pointsNumber; i++)
        {
            line.AddPoint(newPos);
            velocity.y += Globals.Gravity * delta;
            newPos += velocity * delta;

            //float lineAngle = i > 0 ? line.GetPointPosition(i - 1).DirectionTo(line.GetPointPosition(i)).Angle() : Mathf.Deg2Rad(degAngle);
            //if (IsCollidingShape(ToGlobal(newPos), lineAngle)) break;
            if (IsCollidingPoint(ToGlobal(newPos))) break;

        }
    }

/*     protected bool IsCollidingShape(Vector2 position, float angle)
    {
        Physics2DShapeQueryParameters queryParameters = new()
        {
            Transform = new Transform2D(angle, position)
        };


        queryParameters.SetShape(collisionShape);
        Physics2DDirectSpaceState spaceState = GetWorld2d().DirectSpaceState;

        var queryResult = spaceState.IntersectShape(queryParameters);

        foreach (Godot.Collections.Dictionary result in queryResult)
        {
            if(result["collider"] is Jugador jugador && jugador!=Inventory.SelectedPlayer) // si colisiona con un jugador y el jugador no es el seleccionado
            {
                canThrow=false;
            }

            if (result["collider"] is not KinematicBody2D)
            {
                return true;
            }
        }

        return false;
    } */

    protected bool IsCollidingPoint(Vector2 position)
    {
        Physics2DDirectSpaceState spaceState = GetWorld2d().DirectSpaceState;
        var queryResult=spaceState.IntersectPoint(position);

        foreach (Godot.Collections.Dictionary result in queryResult)
        {

/*             if(result["collider"] is Jugador jugador && jugador!=Inventory.SelectedPlayer) // si colisiona con un jugador y el jugador no es el seleccionado
            {
                canThrow=false;
            } */

            if (result["collider"] is not KinematicBody2D)
            {
                return true;
            }
        }

        return false;
    }


}
