using Godot;
using System;
using System.Collections.Generic;

public class Thrower : ProjectileLauncher
{
    private Vector2 offset = new Vector2(0, 35);
    private float maxSpeed = 800;

    private Vector2 startPos = Vector2.Zero;

    public Throwable throwable;
    //tiene que ser del throwable porque sino, da (0,0) porque se mueve junto con el mouse y pasan
    //cosas raras
    private Vector2 localMousePos{get=>throwable.GetLocalMousePosition();}


    protected override Vector2 StartingPoint { get => startPos-Position; }

    General signalManager;

    public override void _Ready()
    {
        base._Ready();
        signalManager=GetNode<General>("/root/General");
        //Position = throwable.GlobalPosition;
        //Position = throwable.Position;
        Position += offset;
    }

    public override void _Process(float delta)
    {
        if (selected)
        {
            Position = localMousePos;
			
			if (Input.IsActionJustReleased("LeftClick"))
            {
/* 				if(!CorrectAngle() && throwable is not Jugador)
				{
					RestartLaunch();
					return;
				} */
                MouseReleased();
            }
        }
/*         GetNode<Sprite>("Sprite2").Position=Vector2.Zero;
        GD.Print("GetLocalMousePosition : "+GetGlobalMousePosition());
        GD.Print("localMousePos: "+localMousePos); */

    }


    protected override void CalculateInitialVelocity()
    {
        direction = (startPos -localMousePos).Normalized();
        speed = Mathf.Clamp(startPos.DistanceTo(localMousePos) * 2, 0, maxSpeed);  //*2 es un ajuste
        float angle = direction.Angle();
		degAngle=Mathf.Rad2Deg(angle);
        Vector2 velocity = direction * speed;
        initialVelocity = direction * speed;
    }

    private void MouseReleased()
    {
        selected = false;

        if (collidingBodies.Count > 0 || !canThrow)
        {
            RestartLaunch();
            return;
        }

        throwable.SetVelocity(initialVelocity);
        signalManager.EmitSignal(nameof(General.OnThrowableLaunched), throwable);
        QueueFree();


        if(throwable is Jugador player)
        {
            player.BoutaMove=false;
            if(player.OnMovingPlatform!=null)
            {
                Vector2 offset=new((float)(MovingPlatform.Speed*(player.OnMovingPlatform*-1)), 0);
                //GD.Print(offset);

                initialVelocity+=offset;
            }

			Inventory.Unopenable=false;

            return;
        }


        GetTree().CallGroup("Escenarios", "ChangeTurn");
    }

    protected override void RestartLaunch()
    {
		selected=false;
        //Position = throwable.Position;
        Position = Vector2.Zero;
        Position += offset;
        line.ClearPoints();
        RemoveCollisions();
        restartSound.Play();
    }


    private void _on_Thrower_input_event(object viewport, object @event, int shape_idx)
    {
        if (@event is InputEventMouseButton MouseButtonEvent)
        {
            if (MouseButtonEvent.ButtonIndex == (int)ButtonList.Left)
            {
                if (MouseButtonEvent.Pressed)
                {
                    selected = true;
                }
            }
        }
    }


	public static Thrower GetThrower(Throwable _throwable)
	{
		PackedScene lanzador=(PackedScene)ResourceLoader.Load("res://scenes/Thrower.tscn");
		Thrower thrower=(Thrower)lanzador.Instance();
		thrower.throwable=_throwable;

		return thrower;
	}

	public static Thrower GetThrower(Throwable _throwable, float _lineWidth)
	{
		PackedScene lanzador=(PackedScene)ResourceLoader.Load("res://scenes/Thrower.tscn");
		Thrower thrower=(Thrower)lanzador.Instance();
		thrower.throwable=_throwable;
		thrower.lineWidth=_lineWidth;

		return thrower;
	}

}
