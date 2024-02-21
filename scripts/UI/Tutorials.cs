using Godot;
using System;

public class Tutorials : CanvasLayer
{
    Label title;
    ItemList itemList;
    TextureButton backButton, forwardButton;

    readonly string[][] items=new string[][]  //first is the title
    {
        new string[]
        {
            "Empezar partida",
            "Presiona el botón de empezar",
            "Empieza una nueva partida o \ncontinúa una       previamente guardada",
            "Selecciona el inventario del equipo acordado   con tu compañero",
            "Si pasas el ratón por una herramienta podrás   ver su descripción",
            "Una vez terminado, ambos presionen los       botones de Listo"
        },

        new string[]
        {
            "Inventario",
            "En tu turno, has clic en uno de los miembros de tu equipo para abrir el inventario",
            "La compra y venta funciona igual que en el   menú de selección de inventario",
            "Has clic en los íconos para seleccionar",
            "Usa la herramienta",
            "Puedes cancelar su lanzamiento presionando Q o la rueda del mouse o el botón si estás en móvil",
        },

        new string[]
        {
            "Herramientas arrojadizas",
            "Cuando veas un círculo debajo del integrante,  es que vas a usar una herramienta arrojadiza",
            "Arrastra el ratón para apuntar, y suelta para  lanzar",
            "No puedes apuntar directamente a otro         integrante"
        },

        new string[]
        {
            "Lanzaglobos",
            "Arrastra el ratón sobre la barra deslizadora   para apuntar, y suelta para lanzar uno de   sus globos, puedes lanzar 3 por turno",
            "No puedes apuntar directamente a otro        integrante"

        },

        new string[]
        {
            "Globo teledirigido",
            "Controla el globo mediante las teclas WASD o las de dirección",
            "También puedes mantener pulsado el ratón para mover el globo hacia esa dirección",
            "El globo no puede reventar directamente en un integrante, tienes que reventarlo en una     plataforma"
        },

        new string[]
        {
            "Puntos de humedad",
            "Todos los integrantes tienen un medidor de   humedad",
            "Cuando asestas a un integrante con un globo, su medidor subirá",
            "Cuando el medidor esté lleno, el integrante  desaparecerá de la partida"
        },

        new string[]
        {
            "Menú de pausa",
            "Presiona el botón de pausa o Esc en cualquier momento para abrir el menú de pausa durante  la partida",
            "Mientras este menú este abierto, el juego    estará completamente pausado",
            "Presiona a la opción de reanudar o Esc para  salir del menú de pausa y regresar a la       partida"
        },


        new string[]
        {
            "¿Cómo ganar?",
            "Cuando el indicador de humedad de un miembro se llene o si el miembro sale del escenario,  se retirará del juego, disminuyendo así el     recuento total de miembros en tu equipo",
            "Una vez que el contador de un equipo llegue a cero, el otro será declarado ganador de la   partida"
        },

        new string[]
        {
            "Habilidades especiales",
            "Cada 3 cambios de turno, ambos equipos       desbloquearán habilidades especiales que      pueden ser activadas presionando el botón",
            "La habilidad de los marcianos les permite    hacerse invisibles por un turno",
            "La habilidad de los astronautas les permite  invocar una nave que suelta un globo en        cualquier punto del escenario desde una      altura fija"
        }

    };

    VScrollBar scrollBar;
    byte currentIndex=0;


    public override void _Ready()
    {
        title=GetNode<Label>("CenterContainer/Title");
        itemList=GetNode<ItemList>("ItemList");
        backButton=GetNode<TextureButton>("BackButton");
        forwardButton=GetNode<TextureButton>("ForwardButton");

        backButton.Connect("pressed", this, nameof(_on_Button_pressed), new Godot.Collections.Array{true});
        forwardButton.Connect("pressed", this, nameof(_on_Button_pressed), new Godot.Collections.Array{false});

        SetItems();
        scrollBar= itemList.GetVScroll();
    }

    private void SetItems()
    {
        string[] strings=items[currentIndex];

        itemList.Clear();
        title.Text=strings[0];

       for (int i = 1; i < strings.Length; i++)
        {
            var pngPath = $"res://sprites/Tutorials/{currentIndex}{i}.png";
            var tresPath = $"res://sprites/Tutorials/{currentIndex}{i}.tres";

            if(Globals.MobileDevice)
            {
                strings[i] = strings[i].Replace("ratón", "dedo");
            }

            if(ResourceLoader.Exists(pngPath))
            {
                itemList.AddItem(strings[i], GD.Load<Texture>(pngPath));
                continue;
            }

            if(ResourceLoader.Exists(tresPath))
            {
                itemList.AddItem(strings[i], GD.Load<AnimatedTexture>(tresPath));
                continue;
            }

            itemList.AddItem(strings[i]);
        }
    }


    private void _on_Button_pressed(bool isBackButton)
    {
        int limit= isBackButton ? 0 : items.Length-1;

        if(currentIndex==limit)
        {
            return;
        }

        currentIndex= (byte)(isBackButton ? (byte)currentIndex-1 : (byte)currentIndex+1);
        SetItems();

        backButton.Disabled=currentIndex==0;
        forwardButton.Disabled=currentIndex==items.Length-1;

        scrollBar.Value=0;

    }

    private void _on_Close_pressed()
    {
        QueueFree();
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if(@event is InputEventKey inputEventKey && inputEventKey.Pressed)
        {
            if(inputEventKey.Scancode ==(int)KeyList.Left)
            {
                _on_Button_pressed(true);
            }

            if(inputEventKey.Scancode ==(int)KeyList.Right)
            {
                _on_Button_pressed(false);
            }
        }
    }

    public static Tutorials GetTutorials()
    {
        PackedScene packedScene=GD.Load<PackedScene>("res://scenes/UI/Tutorials.tscn");
        Tutorials tutorials=packedScene.Instance<Tutorials>();

        return tutorials;
    }
}
