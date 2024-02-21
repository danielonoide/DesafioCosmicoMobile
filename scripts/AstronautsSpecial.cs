using Godot;
using System;

public class AstronautsSpecial : Area2D
{

    RectangleShape2D rectangleShape2D;
    Line2D line;
    AudioStreamPlayer launchRestart;

    public override void _Ready()
    {
        Inventory.Unopenable=true;
        line=GetNode<Line2D>("Line2D");
        launchRestart=GetNode<AudioStreamPlayer>("LaunchRestartSound");

        rectangleShape2D = new()
        {
            Extents = new Vector2(16, 1)
        };

        if(Globals.MobileDevice)
        {
            GetNode<TextureButton>("CanvasLayer/LaunchBTN").Visible = true;
        }
    }



    private void DrawLine()
    {
        line.ClearPoints();
        line.AddPoint(Vector2.Zero);
        line.AddPoint(ToLocal(CanDrop()));
    }

    private Vector2 LineEnding() //poca precisión
	{
        Physics2DDirectSpaceState spaceState = GetWorld2d().DirectSpaceState;
        Vector2 bottomLimit=new(Position.x, 1200);
		Godot.Collections.Dictionary queryResult=
		spaceState.IntersectRay(Position, bottomLimit, new Godot.Collections.Array{this}, 1);

        if(queryResult.Count==0) return bottomLimit;
        
        Vector2 result=(Vector2)queryResult["position"];
        result.y-=80; //nose porq

        return result;

	}


    private Vector2 CanDrop()
    {
        Vector2 position=GlobalPosition;

        while(position.y<1200)
        {
            Physics2DShapeQueryParameters queryParameters = new()
            {
                Transform = new Transform2D(0, position)
            };

            queryParameters.SetShape(rectangleShape2D);
            Physics2DDirectSpaceState spaceState = GetWorld2d().DirectSpaceState;

            var queryResult = spaceState.IntersectShape(queryParameters);

            foreach (Godot.Collections.Dictionary result in queryResult)
            {
                if (result["collider"] is not KinematicBody2D || result["collider"] is MovingPlatform)
                {
                    position.y-=80;
                    return position;
                }
            }

            position.y+=10;
        }

        position.y-=80;
        return position;

    }


    private void Drop()
    {
        GetTree().CallGroup("Escenarios", "ChangeTurn");
        LaunchBalloon();
        QueueFree();
    }

    private void _on_LaunchBTN_pressed()
    {
        Drop();
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if(@event is InputEventMouseButton mouseButton && mouseButton.ButtonIndex==(int)ButtonList.Left && mouseButton.Pressed
        && !Globals.MobileDevice
        )
        {
            Drop();
        }


        if(@event is InputEventMouseButton)
        {
            Position=new Vector2(GetGlobalMousePosition().x, Position.y);
            DrawLine();
        }

        if(@event is InputEventMouseMotion)
        {
            Position=new Vector2(GetGlobalMousePosition().x, Position.y);
            DrawLine();
        }
    }

    private void LaunchBalloon()
    {
        GloboConAgua globoConAgua=GloboConAgua.GetSpecialWaterBalloon();

        globoConAgua.Position=GlobalPosition;
        globoConAgua.SetVelocity(new Vector2(0,1));

        Escenario escenario=GetTree().Root.GetNode<Escenario>("Escenario");
        escenario.AddChild(globoConAgua);
    }


    public static AstronautsSpecial GetAstronautsSpecial()
    {
        PackedScene scene=(PackedScene)ResourceLoader.Load("res://scenes/AstronautsSpecial.tscn");
        AstronautsSpecial astronautsSpecial=(AstronautsSpecial)scene.Instance();
        return astronautsSpecial;
    }
}
