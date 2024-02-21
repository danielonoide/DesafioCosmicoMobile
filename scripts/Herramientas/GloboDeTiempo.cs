using Godot;
using System;

public class GloboDeTiempo : GloboConAgua
{
    Timer timeToExplode;
    TextureButton addButton;
    TextureButton subtractButton;
    Label timeLabel;

    Control selector;

    byte time=minTime;
    const byte maxTime=10;
    const byte minTime=2;

    AudioStreamPlayer clockTickingSound;
    bool clockTickingSoundPlayed=false;


    public override void _Ready()
    {
        baseSpeed=baseSpeed*2;
        explosionRadius=explosionRadius*2;
        base._Ready();

        clockTickingSound=GetNode<AudioStreamPlayer>("ClockTicking");
        timeToExplode=GetNode<Timer>("TimeToExplode");
        addButton=GetNode<TextureButton>("Selector/Add");
        subtractButton=GetNode<TextureButton>("Selector/Subtract");
        timeLabel=GetNode<Label>("Selector/Time");
        selector=GetNode<Control>("Selector");

        if(Globals.MobileDevice)
        {
            selector.RectScale *= 3;
        }
        
    }

    public override void _PhysicsProcess(float delta)
    {
        base._PhysicsProcess(delta);
        if(velocity!=Vector2.Zero)
        {
            if(!clockTickingSoundPlayed) 
            {
                clockTickingSound.Play();
                clockTickingSoundPlayed=true;
            }
            
            if(timeToExplode.IsStopped()) timeToExplode.Start();
            selector.Visible=false;
        }
    }


    protected new void _on_Collision_body_entered(Node body)
    {
        if(body is KinematicBody2D)
        {
            return;
        }
        velocity=new Vector2(0,0);

    } 

    private void _on_TimeToExplode_timeout()
    {
        Explode();
    }

    private void _on_Subtract_pressed()
    {
        if(time>minTime)
        {
            time-=1;
            timeLabel.Text=time.ToString();
            timeToExplode.WaitTime=time;
        }
    }

    private void _on_Add_pressed()
    {
        if(time<maxTime)
        {
            time+=1;
            timeLabel.Text=time.ToString();
            timeToExplode.WaitTime=time;
        }
    }



}
