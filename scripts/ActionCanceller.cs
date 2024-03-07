using Godot;
using System;

public class ActionCanceller : CanvasLayer
{

    byte tool;
    bool cancelMovement=false;

    public byte Tool
    {
        get=>tool;

        set
        {
            if(value>=0 && value<=9)
            {
                tool=value;
            }
            else
            {
                throw new ArgumentOutOfRangeException(value.ToString(), "número de herramienta inválido");
            }
        }
    }

    TextureButton cancelButton;
    Rect2 cancelButtonRect2;
    General signalManager;

    public override void _Ready()
    {
        signalManager=GetNode<General>("/root/General");
        signalManager.Connect(nameof(General.OnTurnChanged), this, nameof(OnTurnChanged));
        signalManager.Connect(nameof(General.OnThrowableLaunched), this, nameof(OnThrowableLaunched));

        if(!Globals.MobileDevice)
        {
            GetNode<Label>("Label").Visible = true;
        }
        else
        {
            cancelButton = GetNode<TextureButton>("CancelBTN");
            cancelButton.Visible = true;
            cancelButtonRect2 = new(cancelButton.RectPosition, cancelButton.RectSize * cancelButton.RectScale);
        }

    }

    public void Cancel()
    {
        ProjectileLauncher.selected = false;
        if(cancelMovement)
        {
            CancelMovement();
        }
        else
        {
            CancelTool();
        }
    }


    public void CancelTool()
    {
        Inventory.Unopenable=false;
        
        //restar herramienta
        Inventory.SelectedPlayer.ToolsAvailable[tool]++;
        Inventory.SelectedPlayer.AddChild(Inventory.GetInventory());

        if(!Inventory.SelectedPlayer.Moved)
        {
            Inventory.SelectedPlayer=null;
        }

       DeleteTools();
       QueueFree();
    }

    public void CancelMovement()
    {
        Inventory.Unopenable=false;
        Inventory.SelectedPlayer.Moved=false;
        Inventory.SelectedPlayer.BoutaMove=false;
        Inventory.SelectedPlayer.AddChild(Inventory.GetInventory());
        Inventory.SelectedPlayer=null;


        DeleteThrower();
        QueueFree();
    }

    private void DeleteThrower()
    {
        var thrower=GetTree().GetNodesInGroup("Thrower");
        foreach(Thrower th in thrower)
        {
            th.QueueFree();
        }

    }

    private void DeleteTools()
    {
        var throwables=GetTree().GetNodesInGroup("Throwable");

        foreach(Throwable throwable in throwables)
        {
            if ((throwable is Platano platano && platano.dropped) ||
                (throwable is Iman iman && iman.launched ) ||
                (throwable is Teleporter teleporter1 && teleporter1.launched))
            {
                continue;
            }

            if(throwable is GloboTeledirigido globoTeledirigido)
            {
                signalManager.EmitSignal(nameof(General.OnRemoteBalloonRemoved), globoTeledirigido);
            }

            if(throwable is Teleporter teleporter)
            {
                signalManager.EmitSignal(nameof(General.OnTeleporterRemoved), teleporter);
            }


            throwable.QueueFree();
        }

        //eliminar lanzaglobos
        var lanzaglobos=GetTree().GetNodesInGroup("Lanzaglobos");
        foreach(Lanzaglobos lanzaglobos1 in lanzaglobos)
        {
            lanzaglobos1.QueueFree();
        }

    }

/*     private void _on_CancelBTN_pressed()
    {
        Cancel();
    } */

    private void OnTurnChanged(bool martianTurn)
    {
        QueueFree();
    }

    private void OnThrowableLaunched(Throwable throwable)
    {
        QueueFree();
    }

    
    public override void _Input(InputEvent @event)
    {
        if(@event is InputEventKey inputEventKey && inputEventKey.Scancode==(int)KeyList.Q && inputEventKey.Pressed)
        {
            Cancel();
        }

        if(@event is InputEventMouseButton mouseButton && mouseButton.Pressed && mouseButton.ButtonIndex==(int)ButtonList.Middle)
        {
            Cancel();
        }

        if(@event is InputEventScreenTouch screenTouch && screenTouch.Pressed && 
        Globals.MobileDevice && cancelButtonRect2.HasPoint(screenTouch.Position))
        {
            Cancel();
        }
    }




    public static ActionCanceller GetToolCanceller(byte _tool)
    {
        PackedScene packedScene=GD.Load<PackedScene>("res://scenes/ToolCanceller.tscn");
        ActionCanceller actionCanceller=packedScene.Instance<ActionCanceller>();
        actionCanceller.Tool=_tool;

        return actionCanceller;
    }

    public static ActionCanceller GetMovementCanceller()
    {
        PackedScene packedScene=GD.Load<PackedScene>("res://scenes/ToolCanceller.tscn");
        ActionCanceller actionCanceller=packedScene.Instance<ActionCanceller>();
        actionCanceller.cancelMovement=true;

        return actionCanceller;
    }

}
