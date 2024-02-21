using Godot;
using System;

public class Lanzaglobos : ProjectileLauncher, IPersist
{
    Sprite sprite;
    HSlider hSlider;
    Label label;
    float offset=23.5f;
    const byte maxBaloons=3;
    public byte BalloonsExploded {get; set;}=0;

    public byte balloonsLaunched=0;
    bool balloonExploded=false;

    protected override Vector2 StartingPoint { get => Vector2.Zero; }

    Escenario escenario;
    General signalManager;


    public override void _Ready()
    {
        base._Ready();
        signalManager=GetNode<General>("/root/General");
        escenario=GetTree().Root.GetNode<Escenario>("Escenario");


        sprite=GetNode<Sprite>("Sprite");   
        hSlider=GetNode<HSlider>("HSlider");
        label=hSlider.GetNode<Label>("Label");

        lineWidth=49;
        line.Width=lineWidth;
        collisionShape.Extents = new Vector2(1, lineWidth / 2);


        speed=500;
    }

    private GloboConAgua LaunchBalloon()
    {
        GloboConAgua globoConAgua=GloboConAgua.GetWaterBalloon();

        globoConAgua.SetVelocity(initialVelocity);
        globoConAgua.Position=GlobalPosition;

        signalManager.EmitSignal(nameof(General.OnThrowableLaunched), globoConAgua);

        escenario.AddChild(globoConAgua);
        return globoConAgua;
    }

    protected override void CalculateInitialVelocity()
    {
        float degAngleRadians=Mathf.Deg2Rad(degAngle);
        direction=new Vector2(Mathf.Cos(degAngleRadians), Mathf.Sin(degAngleRadians));
        direction=direction.Normalized();

        initialVelocity=direction*speed;
    }


    protected override void RestartLaunch()
    {
        restartSound.Play();
    }

    public Godot.Collections.Dictionary<string, object> Save()
    {
        return new Godot.Collections.Dictionary<string, object>()
        {
            {"Filename", Filename},
            {"Parent", GetParent().GetPath()},
            {"balloonsLaunched", balloonsLaunched}
        };
    }

    private void _on_HSlider_value_changed(float value)
    {
        degAngle=-value;
        sprite.RotationDegrees=degAngle+offset;
        label.Text=value.ToString();
    }

    private void _on_HSlider_gui_input(InputEvent @event)
    {
        if(@event is InputEventMouseButton mouseButton && mouseButton.ButtonIndex==(int)ButtonList.Left)
        {
            if(mouseButton.Pressed)
            {
                selected=true;
            }
            else 
            {
                selected=false;
/*                 if(!CorrectAngle() || collidingBodies.Count>0 || !canThrow)
                {
                    RestartLaunch();
                    return;
                } */

/*                 if(collidingBodies.Count>0 || !canThrow)
                {
                    RestartLaunch();
                    return;
                } */

                balloonsLaunched++;
                GloboConAgua throwedBalloon=LaunchBalloon();
                if(balloonsLaunched>=3)
                {
                    throwedBalloon.LanzaglobosTerminado=true;
                    QueueFree();
                    GetTree().CallGroup("Escenarios", "ChangeTurn");                    
                }
            }
        }
    }

}
