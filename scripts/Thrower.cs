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
        selected = false;
        signalManager=GetNode<General>("/root/General");
        //Position = throwable.GlobalPosition;
        //Position = throwable.Position;
        Position += offset;


        if(Globals.MobileDevice)
        {
            var collisionShape2D = GetNode<CollisionShape2D>("CollisionShape2D");
            var shape = collisionShape2D.Shape as CircleShape2D;
            shape.Radius *= 1.1f;
            GetNode<Sprite>("Sprite").Scale = new Vector2(0.5f, 0.5f);
        }
    }

    public override void _Process(float delta)
    {
        if (selected)
        {
            Position = localMousePos;
			
			if (Input.IsActionJustReleased("LeftClick"))
            {
                MouseReleased();
            }
        }

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
        throwable.SetVelocity(initialVelocity);
        signalManager.EmitSignal(nameof(General.OnThrowableLaunched), throwable);
        QueueFree();


        if(throwable is Jugador player)
        {
            player.BoutaMove=false;
            if(player.OnMovingPlatform!=null)
            {
                Vector2 offset=new((float)(MovingPlatform.Speed*(player.OnMovingPlatform*-1)), 0);

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
