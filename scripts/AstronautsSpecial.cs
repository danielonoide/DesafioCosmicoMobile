using Godot;
using System;

public class AstronautsSpecial : Area2D
{

    RectangleShape2D rectangleShape2D;
    Line2D line;
    AudioStreamPlayer launchRestart;

    bool canDrop;

    bool mouseMoving=false;
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

    public override void _Process(float delta)
    {
        //GD.Print(canDrop);
        if(mouseMoving)
        {
            Position=new Vector2(GetGlobalMousePosition().x, Position.y);
        }
        mouseMoving=false;
    }


    public override void _PhysicsProcess(float delta)
    {
        if(mouseMoving)
        {
            DrawLine();
        }
    }

    private void DrawLine()
    {
        line.ClearPoints();
        line.AddPoint(Vector2.Zero);
        line.AddPoint(ToLocal(CanDrop(out canDrop)));
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


    private Vector2 CanDrop(out bool canDropParam)
    {
        Vector2 position=GlobalPosition;
        bool playerTouched=false;
        

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
                if(result["collider"] is Jugador)
                {
                    playerTouched=true;
                }

                if (result["collider"] is not KinematicBody2D || result["collider"] is MovingPlatform)
                {
                    canDropParam=!playerTouched;
                    position.y-=80;
                    return position;
                }
            }

            position.y+=10;
        }

        canDropParam=!playerTouched;
        position.y-=80;
        return position;

    }

/*         private bool CanDrop() 
    {
        Vector2 position=GlobalPosition;
        bool platformTouched=false;

        while(position.y<1200)
        {
            Physics2DShapeQueryParameters queryParameters = new Physics2DShapeQueryParameters();
            queryParameters.Transform = new Transform2D(0, position);

            queryParameters.SetShape(rectangleShape2D);
            Physics2DDirectSpaceState spaceState = GetWorld2d().DirectSpaceState;

            var queryResult = spaceState.IntersectShape(queryParameters);

            foreach (Godot.Collections.Dictionary result in queryResult)
            {
                if(result["collider"] is Jugador && !platformTouched)
                {
                    return false;
                }

                if (result["collider"] is not KinematicBody2D)
                {
                    platformTouched=true;
                }
            }

            position.y++;
        }

        return true;

    } */


    private void Drop()
    {
        GetTree().CallGroup("Escenarios", "ChangeTurn");
        LaunchBalloon();
        QueueFree();
    }

    private void AttemptDrop()
    {
        if(canDrop)
        {
            Drop();
        }
        else
        {
            launchRestart.Play();
        }
    }

    private void _on_LaunchBTN_pressed()
    {
        AttemptDrop();
    }

    public override void _Input(InputEvent @event)
    {
        if(@event is InputEventMouseButton mouseButton && mouseButton.ButtonIndex==(int)ButtonList.Left && mouseButton.Pressed
        && !Globals.MobileDevice
        )
        {
            AttemptDrop();
        }

        if(@event is InputEventMouseMotion || @event is InputEventScreenTouch)
        {
            mouseMoving=true;
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
