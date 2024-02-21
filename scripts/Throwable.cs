using Godot;
using System;

public abstract class Throwable : KinematicBody2D, IPersist
{
    // Declare member variables here. Examples:
    // private int a = 2;
    // private string b = "text";

    // Called when the node enters the scene tree for the first time.
    protected Vector2 velocity=new Vector2(0,0);
    public abstract float MaxSize {get; }
    //protected int speed;
    public override void _Ready()
    {
        
    }

	public override void _PhysicsProcess(float delta) //se ejecuta 60 veces por segundo
	{
        Movement(delta);

        if(IsOnFloor()) 
		{
			velocity.x *= 0.9f;
        }
			
    }


    protected virtual void Movement(float delta)
    {
        velocity=MoveAndSlide(velocity, Vector2.Up);
        velocity.y+=Globals.Gravity*delta;
    }

	public void SetVelocity(Vector2 vector)
	{
		this.velocity=vector;
	} 

    public Vector2 GetVelocity()
    {
        return velocity;
    }


    public virtual Godot.Collections.Dictionary<string,object> Save()
    {
        return new Godot.Collections.Dictionary<string, object>()
        {
            {"Filename", Filename},
            {"Position", Position},
            {"velocity", velocity},
        };
    }

}
