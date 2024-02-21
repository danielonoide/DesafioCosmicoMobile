using Godot;
using System;
using System.Collections.Generic;

public class GloboTeledirigido : GloboConAgua
{
    readonly float speed=400;
    bool exploded=false;

    AudioStreamPlayer2D startingSound;
    General signalManager;

    public override void _Ready()
    {
        base._Ready();
        startingSound=GetNode<AudioStreamPlayer2D>("StartingSound");
        signalManager=GetNode<General>("/root/General");

        //que la camara lo siga
        GetTree().CallGroup("Escenarios", "SetCamera", this);
    }

    public override void _PhysicsProcess(float delta)
    {
        if(!exploded) Movement(delta);
        
    }

    protected override void Movement(float delta)
    {
        velocity.x=Convert.ToInt32(Input.IsActionPressed("Right")) - Convert.ToInt32(Input.IsActionPressed("Left"));
		velocity.y=Convert.ToInt32(Input.IsActionPressed("Down")) - Convert.ToInt32(Input.IsActionPressed("Up"));
        

        if(Input.IsActionPressed("LeftClick"))
        {
            velocity=Position.DirectionTo(GetGlobalMousePosition()).Normalized();
        }


		velocity.x*=speed;
		velocity.y*=speed;


        var collisionInfo= MoveAndCollide(velocity*delta, testOnly:true); //solo es para ver si va a colisionar

        if(collisionInfo==null) //si no colisiona entonces si se mueve
        {
            MoveAndSlide(velocity);
        }
    }

    protected new void _on_Collision_body_entered(Node body)
    {
        if(body is Jugador || body is GloboTeledirigido)
        {
            return;
        }

        signalManager.EmitSignal(nameof(General.OnRemoteBalloonRemoved), this);

        startingSound.Stop();
        base._on_Collision_body_entered(body);
        GetTree().CallGroup("Escenarios", "ChangeTurn");
        exploded=true;
    }

    protected new void _on_Explosion_body_entered(Node body)
    {
        if(body is Jugador jugador)
        {
            float distance=jugador.GlobalPosition.DistanceTo(GlobalPosition);
            jugador.AddHumidity(GetHumidityPoints(distance));
            Push(jugador, distance);
            GetTree().CallGroup("Escenarios", "AddStar", jugador.IsMartian, true);
        }
    }
}
