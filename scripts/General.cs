using Godot;
using System;
using System.Linq;

public static class Constants
{
	public static string[] SaveFileNames=new string[3]{"user://Escenario1.save","user://Escenario2.save","user://Escenario3.save"};
	public enum WinningTeam
	{
		Draw, 
		Astronauts,
		Martians
	}
	public enum Gravities
	{
		MarsGravity=500, //óptimo 600 500
		SpaceGravity=125, //óptimo 500  125
		MoonGravity=250 //óptimo 700  250
	};

	public const string MainMenuPath="res://scenes/UI/MainMenu.tscn";
/* 	public const int MarsGravity=600; //óptimo 600
	public const int SpaceGravity=500; //óptimo 500
	public const int MoonGravity=700; //óptimo 700 */
}

static class Volume
{
	public static float[] Volumes=new float[]{1,1,1};
	//1 es general, 2 es musica y 3 es sfx
}

static class Globals
{
	public static bool MobileDevice = true;
	private static int gravity;

	public static int Gravity
	{
		get 
		{
			return gravity;
		}
		set
		{
			if(value>0 && Enum.IsDefined(typeof(Constants.Gravities), value))
			{
				gravity=value;
			}
		}
	}
}

public class EventManager
{
	public delegate void TurnChangedEventHandler(bool isMartianTurn);
	public static event TurnChangedEventHandler OnTurnChanged;

	public static void NotifyTurnChanged(bool isMartianTurn)
	{
		OnTurnChanged?.Invoke(isMartianTurn);
	}

/* 	public delegate void TeleporterRemovedEventHandler(Teleporter teleporter);
	public static event TeleporterRemovedEventHandler OnTeleporterRemoved;

	public static void NotifyTeleporterRemoved(Teleporter teleporter)
	{
		OnTeleporterRemoved?.Invoke(teleporter);
	} */

	//Action clase genérica que representa un delegado sin retorno

/* 	public delegate void PlayerDeathEventHandler(Jugador teleporter);
	public static event PlayerDeathEventHandler OnPlayerDeath;

	public static void NotifyPlayerDeath(Jugador player)
	{
		OnPlayerDeath?.Invoke(player);
	} */

/* 	public event Action<Jugador> OnPlayerDeath;
	public void NotifyPlayerDeath(Jugador player)
	{
		OnPlayerDeath?.Invoke(player);
	}

	public static void SuscribeToOnPlayerDeath(Action<Jugador> method)
	{
		EventManager eventManager=new();
		eventManager.OnPlayerDeath+=method;
	} */


/* 	public delegate void BalloonExplodedEventHandler(GloboConAgua balloon);
	public static event BalloonExplodedEventHandler OnBalloonExploded;

	public static void NotifyBalloonExploded(GloboConAgua balloon)
	{
		OnBalloonExploded?.Invoke(balloon);
	} */
}


public class General : Node2D
{
	[Signal]
	public delegate void OnPlayerDeath(Jugador player);

	[Signal]
	public delegate void OnTurnChanged(bool isMartianTurn);

	[Signal]
	public delegate void OnBalloonExploded(GloboConAgua balloon);

	[Signal]
	public delegate void OnTeleporterRemoved(Teleporter teleporter);


	[Signal]
	public delegate void OnMagnetRemoved(Iman magnet);

	[Signal]
	public delegate void OnRemoteBalloonRemoved(GloboTeledirigido globoTeledirigido);

	[Signal]
	public delegate void OnScreenStatusChanged(bool fullscren);

	[Signal]
	public delegate void OnThrowableLaunched(Throwable throwable);

    public override void _Ready()
    {
		if(!OS.GetName().Equals("Android") && !OS.GetName().Equals("iOS"))
		{
			Globals.MobileDevice = false;
		}
    }


	public override void _Input(InputEvent @event)
    {
        if (@event is InputEventKey eventKey)
        {
            if (eventKey.Pressed && eventKey.Scancode == (int)KeyList.F11)
            {
                OS.WindowFullscreen = !OS.WindowFullscreen;
				EmitSignal(nameof(OnScreenStatusChanged), OS.WindowFullscreen);
            }
        }
    }
}


public class Modify
{

	public static Node2D GetGlobals()
	{
		PackedScene Globals=(PackedScene)ResourceLoader.Load("res://scenes/Globals.tscn");
		return (Node2D)Globals.Instance();
	}
	
	public static void ChangeScale(CanvasLayer Nodo, Vector2 Vector)
	{
		Nodo.Scale=Vector;
	}
	
	public static void ChangeScale(TextureButton Nodo, Vector2 Vector)
	{
		Nodo.RectScale=Vector;
	}
	
	public static void ChangePosition(TextureButton TextB, Vector2 Vector)
	{
		TextB.RectPosition=Vector;
	}
	
	
}

