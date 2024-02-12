using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public class InventorySelection : CanvasLayer
{
    protected byte scenery;

    protected static readonly byte[] starsNumber=new byte[3]{18, 15, 12};
    

    /*
    tool numbers:
     1   "Globo con agua"
     2   "Globo con tinta"
     3   "Globo de hielo"
     4   "Globo de tiempo"
     5   "Globo teledirigido"
     6   "Lanzaglobos"
     7   "Teletransportador"
     8   "Plátano"
     9   "Imán"
    */

    protected static readonly byte[] toolPrices=new byte[9]
    {
        1,3,1,2,3,2,2,2,2
    };

    protected static byte[] astronautsTools=new byte[9]{10,10,10,10,10,10,10,10,10}, martiansTools=new byte[9]{10,10,10,10,10,10,10,10,10};

    public static byte[] AstronautsTools { get => astronautsTools;}
    public static byte[] MartiansTools {get => martiansTools;}


    readonly string[] toolTexts = new string[]
    {
        "Globo con agua: Lanza un globo lleno de agua que reventará al impactar,empapando a los enemigos cercanos y empujándolos. Cuanto más cerca estén, más efecto tendrá. \n(Costo: 1 estrella)",
        
        "Globo con tinta: Lanza un globo lleno de tinta que revienta al impactar, reduciendo la visión de los afectados por un turno. \n(Costo: 3 estrellas)",

        "Globo de hielo: Lanza un globo que al explotar congela a los afectados, dejándolos inmóviles durante un turno. \n(Costo: 1 estrella)",
        
        "Globo de tiempo: Lanza un globo que puedes configurar para que reviente después de un tiempo específico, empapando a los enemigos con más agua que un globo normal. \n(Costo: 2 estrellas)",

        "Globo teledirigido: Controla un globo con agua mediante las teclas WASD, las teclas de dirección o el ratón. Al explotar, empapa a los enemigos cercanos. \n(Costo: 3 estrellas)",
        
        "Lanzaglobos: Lanza globos de agua en la dirección que elijas. Puedes disparar hasta tres globos por turno. \n(Costo: 2 estrellas)",
        
        "Teletransportador: Te permite moverte instantáneamente al lugar donde aterrice en futuros turnos. \n(Costo: 2 estrellas)",
        
        "Plátano: Coloca un plátano en el suelo que hará que los enemigos que lo toquen resbalen y caigan del escenario. \n(Costo: 2 estrellas)",
        
        "Imán: Lanza un imán que atraerá a los enemigos cercanos y los mantendrá atrapados en su área de influencia durante un turno.\n(Costo: 2 estrellas)"
    };

    const float halfScreen=683;

    Label astronautsLabel, martiansLabel;

    protected Label[] counters;

    TextureButton[] readyButtons; //0 astronautas y 1 marcianos

    protected byte astronautsCounter, martiansCounter;

    AcceptDialog acceptDialog;

    public override void _Ready()
    {
        astronautsLabel=GetNode<Label>("AstronautsCounter");
        martiansLabel=GetNode<Label>("MartiansCounter");

        acceptDialog=GetNode<AcceptDialog>("AcceptDialog");

        astronautsCounter=martiansCounter=starsNumber[scenery];
        astronautsLabel.Text=martiansLabel.Text=astronautsCounter.ToString();

        RestartInventories();
        ConfigureButtons();
        ConfigureCounters();
        ConfigureReadyButtons();
        ConfigureTextBoxes();
    }

    protected void RestartInventories()
    {
        //reiniciar inventarios
        astronautsTools=new byte[9]{0,0,0,0,0,0,0,0,0};
        martiansTools=new byte[9]{0,0,0,0,0,0,0,0,0};
    }

    private void ConfigureReadyButtons()
    {
        Godot.Collections.Array readyButtonsGroup=GetTree().GetNodesInGroup("ReadyButtons");
        readyButtons=new TextureButton[2];
        for(int i=0;i<readyButtonsGroup.Count;i++)
        {
            TextureButton readyButton=(TextureButton)readyButtonsGroup[i];
            readyButtons[i]=readyButton;
            readyButton.Connect("pressed", this, nameof(ReadyButtonPressed), new Godot.Collections.Array{i});
        }
    }

    protected void ConfigureCounters()
    {
        var countersGroup=GetTree().GetNodesInGroup("Counters");
        counters=new Label[countersGroup.Count];
        for(int i=0;i<countersGroup.Count;i++)
        {
            counters[i]=(Label)countersGroup[i];
        }
    }

    protected virtual void ConfigureButtons()
    {
        Godot.Collections.Array addButtons=GetTree().GetNodesInGroup("AddButtons");
        Godot.Collections.Array subtractButtons=GetTree().GetNodesInGroup("SubtractButtons");

        for(int i=0;i<addButtons.Count;i++)
        {
            TextureButton addButton=(TextureButton)addButtons[i];
            TextureButton subtractButton=(TextureButton)subtractButtons[i];

            addButton.Connect("pressed", this, nameof(AddTool), new Godot.Collections.Array{i});
            subtractButton.Connect("pressed", this, nameof(SubtractTool), new Godot.Collections.Array{i});
        }
    }

    protected void ConfigureTextBoxes()
    {
        Godot.Collections.Array selectionButtons=GetTree().GetNodesInGroup("SelectButtons");

        for(int i=0;i<selectionButtons.Count;i++)
        {
            Control selectionButton=(Control)selectionButtons[i];

            selectionButton.Connect("mouse_entered", this, nameof(OnToolMouseEntered), new Godot.Collections.Array{i});
            selectionButton.Connect("mouse_exited", this, nameof(OnToolMouseExited));
        }

    }

    //signals
    protected virtual void AddTool(byte tool)
    {
        bool isAstronaut=tool<9;
        byte[] tools=isAstronaut ? astronautsTools:martiansTools;    
        int index=isAstronaut ? tool:tool-9;
        readyButtons[isAstronaut ? 0:1].Disabled=false;

        if(isAstronaut ? astronautsCounter<toolPrices[index] : martiansCounter<toolPrices[index])
        {
            return;
        }

        if(isAstronaut)
        {
            astronautsCounter-=toolPrices[index];
            astronautsLabel.Text=astronautsCounter.ToString();
        }
        else
        {
            martiansCounter-=toolPrices[index];
            martiansLabel.Text=martiansCounter.ToString();
        }

        tools[index]+=1;
        counters[tool].Text=tools[index].ToString();
    }

    protected virtual void SubtractTool(byte tool)
    {
        bool isAstronaut=tool<9;
        byte[] tools=isAstronaut ? astronautsTools:martiansTools;    
        int index=isAstronaut ? tool:tool-9;
        readyButtons[isAstronaut ? 0:1].Disabled=false;

        if(tools[index]<=0)
        {
            return;
        }

        if(isAstronaut)
        {
            astronautsCounter+=toolPrices[index];
            astronautsLabel.Text=astronautsCounter.ToString();
        }
        else
        {
            martiansCounter+=toolPrices[index];
            martiansLabel.Text=martiansCounter.ToString();
        }

        tools[index]-=1;
        counters[tool].Text=tools[index].ToString();
    }

    private void ReadyButtonPressed(byte button)
    {
        readyButtons[button].Disabled=true;
        if(readyButtons[0].Disabled==true && readyButtons[1].Disabled==true)
        {
            StartMatch(scenery);
        }
    }

    private void StartMatch(byte scenery)
    {
        bool astronautsZeroTools = astronautsTools.All(t => t==0);
        bool martiansZeroTools = martiansTools.All(t=> t==0);

        if(!astronautsZeroTools && !martiansZeroTools)
        {
            GetTree().ChangeScene("res://scenes/Escenarios/Escenario"+(scenery+1)+".tscn");
            return;
        }

        acceptDialog.Visible=astronautsZeroTools | martiansZeroTools;
        string dialogText=astronautsZeroTools ? "astronautas" : "marcianos";

        if(astronautsZeroTools && martiansZeroTools)
        {
            dialogText="equipos";
        }

        acceptDialog.DialogText=$"Los {dialogText} no tienen herramientas seleccionadas";

        readyButtons[0].Disabled=!astronautsZeroTools;
        readyButtons[1].Disabled=!martiansZeroTools;
                 
    }

    private void OnToolMouseEntered(int tool)
    {
        if(tool>=9)
        {
            tool-=9;
        }

        bool rightSide=GetViewport().GetMousePosition().x<halfScreen;

        TextBox textBox=TextBox.GetTextBox(toolTexts[tool], rightSide);
        AddChild(textBox);
        textBox.AddToGroup("TextBox");
    }

    private void OnToolMouseExited()
    {
        var textBoxes=GetTree().GetNodesInGroup("TextBox");
        foreach(TextBox textBox in textBoxes)
        {
            textBox.QueueFree();
        }
    }


    private void _on_Close_pressed()
    {
        QueueFree();
    }

	public static InventorySelection GetInventorySelection(byte _scenery)
	{
		PackedScene inventorySelection=(PackedScene)ResourceLoader.Load("res://scenes/UI/InventorySelection.tscn");
        InventorySelection instance=(InventorySelection)inventorySelection.Instance();
        instance.scenery=_scenery;
		return instance;
	}

}
