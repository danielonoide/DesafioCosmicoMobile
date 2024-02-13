using Godot;
using System;
using System.Collections.Generic;


public class Platano : Throwable
{
    RectangleShape2D rectangleShape2D;
    public bool dropped=false;
    AudioStreamPlayer restartSound;
    AudioStreamPlayer soundEffect;
    bool flag=true;
    public bool martianDropped=false;
    public override float MaxSize {get; }

    List<Node> collidingBodies=new();

    public bool loaded=false;

    public CollisionShape2D collisionShape2D;

    public bool detectPlayers=false;
    public TextureButton launchButton; 

    General signalManager;

    public override void _Ready()
    {
        signalManager=GetNode<General>("/root/General");

        restartSound=GetNode<AudioStreamPlayer>("LaunchRestartSound");
        soundEffect=GetNode<AudioStreamPlayer>("SoundEffect");
        collisionShape2D=GetNode<CollisionShape2D>("CollisionShape2D");
        launchButton = GetNode<TextureButton>("CanvasLayer/LaunchBTN");

        rectangleShape2D=new();
        rectangleShape2D.Extents=new Vector2(62,1);

        if(Globals.MobileDevice)
        {
            launchButton.Visible = true;
        }
    }

    public override void _PhysicsProcess(float delta)
    {
        if(velocity!=Vector2.Zero) 
        {
            base._PhysicsProcess(delta);
            detectPlayers=true;
        }

    }

    private bool CanDrop()
    {
        Vector2 position=Position;
        //position-=new Vector2(-60,0);
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

        return collidingBodies.Count==0;

    }


    private void _on_BananaIsColliding_body_entered(Node body)
    {
        collidingBodies.Add(body);

        if(!detectPlayers) return;

        if(body is Jugador jugador )
        {
            if(jugador.IsMartian!=martianDropped)
            {
                jugador.HasToFall=true;
                GetTree().CallGroup("Escenarios", "AddStar", jugador.IsMartian, true);
                GetNode<Sprite>("Sprite").Visible=false;
                soundEffect.Play();
            }
            return;
        }
        
        if(!loaded) GetTree().CallGroup("Escenarios", "ChangeTurn");

        if(body is not MovingPlatform)
        {
            velocity=Vector2.Zero;
        }
        
    }

    private void _on_BananaIsColliding_body_exited(Node body)
    {
        collidingBodies.Remove(body);
    }


    private void _on_SoundEffect_finished()
    {
        QueueFree();
    }

    private void Drop()
    {
        dropped=true;
        collisionShape2D.Disabled=false;
        martianDropped=Escenario.MartianTurn;
        SetVelocity(new Vector2(0,-1));
        signalManager.EmitSignal(nameof(General.OnThrowableLaunched), this);
    }

    private void AttemptDrop()
    {
        if(CanDrop())
        {
            Drop();
            GetNode<TextureButton>("CanvasLayer/LaunchBTN").Visible = false;
        }
        else
        {
            restartSound.Play();
        }
    }

    private void _on_BananaIsColliding_input_event(object viewport, object @event, int shape_idx)
    {
        if(@event is InputEventMouseButton mouseButton && mouseButton.ButtonIndex==(int)ButtonList.Left
         && !mouseButton.Pressed && !dropped && !Globals.MobileDevice)
        {
            AttemptDrop();
        }
    }

    private void _on_LaunchBTN_pressed()
    {
        AttemptDrop();
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if(dropped)
        {
            return;
        }

        if(@event is InputEventMouseButton)
        {
            Position = GetGlobalMousePosition();
        }

        if(@event is InputEventMouseMotion)
        {
            Position = GetGlobalMousePosition();
        }
    }

    public override Godot.Collections.Dictionary<string,object> Save()
    {
        var save=base.Save();
        save.Add("martianDropped", martianDropped);
        save.Add("dropped", dropped);
        return save;
    }

    public static Platano GetPlatano()
	{
		PackedScene scene=(PackedScene)ResourceLoader.Load("res://scenes/Herramientas/Platano.tscn");
		Platano platano=scene.Instance<Platano>();

/*         platano.martianDropped=banana.martianDropped;
        platano.Position=banana.position;         */
        //platano.dropped=true;
        platano._Ready();
        platano.detectPlayers=true;
        platano.collisionShape2D.Disabled=false;
        

		return platano;
	}

}
