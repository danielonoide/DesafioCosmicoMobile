using Godot;
using System;
using System.Collections;
using System.Linq;

public class InventorySelectionMobile : InventorySelection
{
    Label titleLabel, starsLabel;
    AcceptDialog acceptDialog;
    bool astronautsSelecting = true;
    const string AstronautsTitle = "Seleccionar Inventario Astronautas";
    const string MartiansTitle = "Seleccionar Inventario Marcianos";

    public override void _Ready()
    {
        RestartInventories();
        base.ConfigureButtons();
        ConfigureCounters();
        ConfigureTextBoxes();

        titleLabel = GetNode<Label>("TitleContainer/Title");
        starsLabel = GetNode<Label>("Stars/Label");
        acceptDialog = GetNode<AcceptDialog>("AcceptDialog");

        astronautsCounter=martiansCounter=starsNumber[scenery];
        starsLabel.Text = astronautsCounter.ToString();
    }

    private void _on_ReturnBTN_pressed()
    {
        if (astronautsSelecting)
        {
            QueueFree();
            return;
        }

        ChangeTeam();
    }

    private void _on_ContinueBTN_pressed()
    {
        if (astronautsSelecting && astronautsTools.All(t => t == 0) ||
            !astronautsSelecting && martiansTools.All(t => t == 0))
        {
            acceptDialog.Visible = true;
            return;
        }

        if (astronautsSelecting)
        {
            ChangeTeam();
        }
        else
        {
            GetTree().ChangeScene($"res://scenes/Escenarios/Escenario{scenery + 1}.tscn");
        }
    }


    private void ChangeTeam()
    {
        astronautsSelecting = !astronautsSelecting;   

        string title = astronautsSelecting ? AstronautsTitle : MartiansTitle;
        string stars = astronautsSelecting ? astronautsCounter.ToString() : martiansCounter.ToString();

        titleLabel.Text = title;
        starsLabel.Text = stars;


        byte[] currTools = astronautsSelecting ? astronautsTools : martiansTools;


        for(int i=0; i<currTools.Length; i++)
        {
            counters[i].Text = currTools[i].ToString();
        }

    }



    protected override void AddTool(byte toolIndex)
    {
        if (astronautsSelecting)
        {
            if (astronautsCounter < toolPrices[toolIndex])
                return;

            astronautsCounter -= toolPrices[toolIndex];
            astronautsTools[toolIndex]++;
        }
        else
        {
            if (martiansCounter < toolPrices[toolIndex])
                return;

            martiansCounter -= toolPrices[toolIndex];
            martiansTools[toolIndex]++;
        }

        UpdateCounters(toolIndex);
    }

    protected override void SubtractTool(byte toolIndex)
    {
        if (astronautsSelecting)
        {
            if (astronautsTools[toolIndex] <= 0)
                return;

            astronautsCounter += toolPrices[toolIndex];
            astronautsTools[toolIndex]--;
        }
        else
        {
            if (martiansTools[toolIndex] <= 0)
                return;

            martiansCounter += toolPrices[toolIndex];
            martiansTools[toolIndex]--;
        }

        UpdateCounters(toolIndex);
    }

    private void UpdateCounters(byte toolIndex)
    {
        starsLabel.Text = astronautsSelecting ? astronautsCounter.ToString() : martiansCounter.ToString();
        counters[toolIndex].Text = astronautsSelecting ? astronautsTools[toolIndex].ToString() : martiansTools[toolIndex].ToString();
    }

    public static InventorySelectionMobile GetInventorySelectionMobile(byte _scenery)
	{
		PackedScene inventorySelection=(PackedScene)ResourceLoader.Load("res://scenes/UI/InventorySelectionMobile.tscn");
        InventorySelectionMobile instance=(InventorySelectionMobile)inventorySelection.Instance();
        instance.scenery=_scenery;
		return instance;
	}
}
