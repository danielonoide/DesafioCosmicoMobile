using Godot;
using System;

public class ScenerySelection : CanvasLayer
{
    public enum Sceneries 
    {
        Scenery1,
        Scenery2,
        Scenery3,
        Null
    }
    public static Sceneries ActiveScenery{get;set;}=Sceneries.Scenery1;
    public static bool LoadGame{get; set;}=false;
    TextureButton[] startMatchButtons=new TextureButton[3];
    TextureButton[] continueButtons=new TextureButton[3];

    public override void _Ready()
    {
        LoadGame=false;
        var startMatchButtonsArr=new Godot.Collections.Array();
        startMatchButtonsArr=GetTree().GetNodesInGroup("BotonesEmpezarPartida");

        for(int i=0;i<startMatchButtons.Length;i++)
		{
			startMatchButtons[i]=(TextureButton)startMatchButtonsArr[i];
            startMatchButtons[i].Connect("pressed", this, nameof(StartButtonPressed), new Godot.Collections.Array{i});
		}

        Godot.Collections.Array continueButtonsArr=GetTree().GetNodesInGroup("ContinueButtons");
        for(int i=0;i<continueButtonsArr.Count;i++)
        {
            continueButtons[i]=(TextureButton)continueButtonsArr[i];
            continueButtons[i].Connect("pressed", this, nameof(ContinueButtonPressed), new Godot.Collections.Array{i});
        }

        DisableContinueButtons();
    }

    private void DisableContinueButtons()
    {
        File saveGame=new();
        for(int i=0;i<Constants.SaveFileNames.Length; i++)
        {
            if(!saveGame.FileExists(Constants.SaveFileNames[i]))
            {
                continueButtons[i].Disabled=true;
            }
        }
    }

    private void DefineScenery(byte scenery)
    {
        switch(scenery)
        {
            case 0:
                ActiveScenery=Sceneries.Scenery1;
                break;
            case 1:
                ActiveScenery=Sceneries.Scenery2;
                break;
            case 2:
                ActiveScenery=Sceneries.Scenery3;
                break;
        }
    }


    private void ContinueButtonPressed(byte scenery)
    {
        AffirmationScreen affirmationScreen=AffirmationScreen.GetAffirmationScreen(
            AffirmationScreen.Actions.ContinueGame,
            "¿Continuar partida?"
        );

        affirmationScreen.Connect("ContinueGame",this, nameof(OnContinueGame), 
        new Godot.Collections.Array{scenery});
        AddChild(affirmationScreen);
    }

    private void OnContinueGame(byte scenery)
    {
        DefineScenery(scenery);
        LoadGame=true;
        GetTree().ChangeScene("res://scenes/Escenarios/Escenario"+(scenery+1)+".tscn");
    }

    private void StartButtonPressed(byte scenery)
    {
        AffirmationScreen affirmationScreen=AffirmationScreen.GetAffirmationScreen(
            AffirmationScreen.Actions.StartGame,
            "¿Empezar partida?"
        );

        affirmationScreen.Connect("StartGame",this, nameof(OnStartGame), 
        new Godot.Collections.Array{scenery});
        AddChild(affirmationScreen);
    }

    private void OnStartGame(byte scenery)
    {
        DefineScenery(scenery);
        File file=new();

        if(file.FileExists(Constants.SaveFileNames[scenery]))
        {
            Directory directory=new();
            directory.Remove(Constants.SaveFileNames[scenery]);
        }

        if(Globals.MobileDevice)
        {
            InventorySelectionMobile inventorySelectionMobile = InventorySelectionMobile.GetInventorySelectionMobile(scenery);
            AddChild(inventorySelectionMobile);
            DisableContinueButtons();
            return;
        }

        InventorySelection inventorySelection=InventorySelection.GetInventorySelection(scenery);
        AddChild(inventorySelection);   
        DisableContinueButtons();

    }


    private void _on_Close_pressed()
    {
        QueueFree();
    }


    public static CanvasLayer GetScenerySelection()
	{
		PackedScene scenerySelection=(PackedScene)ResourceLoader.Load("res://scenes/UI/ScenerySelection.tscn");
		return (CanvasLayer)scenerySelection.Instance();
	}

}
