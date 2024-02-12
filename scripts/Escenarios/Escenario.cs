using Godot;
using System;
using System.Collections.Generic;
using Newtonsoft.Json;


public partial class Escenario : Node2D
{
	protected Camera2D camera;
	protected Label zoomPercentage;
	protected float zoom=0.1f; //los saltos
	protected bool rightClick=false;
	protected const float maxZoom=0.5f;
	protected const float minZoom=1.9f;

	protected const float realMinZoom=1.95f;
	protected Vector2 cameraSize=new(1366, 768);
	protected float leftLimit=-2500f; 
	protected float rightLimit=2500f;
	protected float topLimit=-1400f;
	protected float bottomLimit=1000f;
	AudioStreamPlayer music;

	static bool martianTurn; 
	public static bool MartianTurn{get=>martianTurn;}

	public static int AstronautsStars { get; set; }
	public static int MartiansStars { get; set; }
	
	byte astronautsSpecialTurnsLeft=3;
	byte martiansSpecialTurnsLeft=3;

	TextureButton astronautsSpecial;
	TextureButton martiansSpecial;

	Control astronautsAddedStars, martiansAddedStars;
	Timer addedStarsTimer;

	bool astronautsSpecialActive=false;

	bool martiansInvisible=false;

	Timer messageTimer;
	Timer gameOverTimer;
	Label messageLabel;

    readonly Texture astronautCursor=GD.Load<Texture>("res://sprites/cursors/spaceship3.png");
    readonly Texture martianCursor=GD.Load<Texture>("res://sprites/cursors/alien_cursor4.png");

    readonly Texture astronautTexture=GD.Load<Texture>("res://sprites/characters/astronaut_idle_single.png");
    readonly Texture martianTexture=GD.Load<Texture>("res://sprites/characters/martian_idle.png");

	AudioStreamPlayer gameOverSound;
	bool gameOverTimerStarted=false;
	AudioStreamPlayer turnChangeSound;

	AudioStreamPlayer deathSound;

	protected Vector2 astronautsCameraPosition=new(0,0);
	protected Vector2 martiansCameraPosition=new(0,0);


	Godot.Collections.Array martians, astronauts;

	Label martiansLabel, astronautsLabel;

	TextureRect ink;

	General signalManager;

	
	public override void _Ready()
	{
		//signals
		signalManager=GetNode<General>("/root/General");
		signalManager.Connect(nameof(General.OnPlayerDeath), this, nameof(OnPlayerDeath)); //nombre de la señal, objetivo y funcion a ejecutar
		signalManager.Connect(nameof(General.OnRemoteBalloonRemoved), this, nameof(OnRemoteBalloonRemoved));


		camera=GetNode<Camera2D>("Camera2D");
		zoomPercentage=GetNode<Label>("HUD/Zoom/Label");
		music=GetNode<AudioStreamPlayer>("Music");
		messageTimer=GetNode<Timer>("HUD/Messaging/Timer");
		messageLabel=GetNode<Label>("HUD/Messaging/CenterContainer/Message");

		//specials
		astronautsSpecial=GetNode<TextureButton>("HUD/TeamInfo/AstronautSpecial");
		martiansSpecial=GetNode<TextureButton>("HUD/TeamInfo/MartianSpecial");

		//game over
		gameOverTimer=GetNode<Timer>("GameOverTimer");

		//ink
		ink=GetNode<TextureRect>("HUD/Ink");


		//audio
		gameOverSound=GetNode<AudioStreamPlayer>("MatchSFX/GameOver");
		turnChangeSound=GetNode<AudioStreamPlayer>("MatchSFX/TurnChange");
		deathSound=GetNode<AudioStreamPlayer>("DeathSound");

		InitializeMembers();

		//initialize stars
		AstronautsStars=0;
		MartiansStars=0;

		//los que muestran las estrellas añadidas
		astronautsAddedStars=GetNode<Control>("HUD/StarsAdded/Astronauts");
		martiansAddedStars=GetNode<Control>("HUD/StarsAdded/Martians");
		addedStarsTimer=GetNode<Timer>("HUD/StarsAdded/Timer");
		

		//team counters
		astronautsLabel=GetNode<Label>("HUD/TeamInfo/AstronautsCounter");
		martiansLabel=GetNode<Label>("HUD/TeamInfo/MartiansCounter");

		astronautsLabel.Text=astronauts.Count.ToString();
		martiansLabel.Text=martians.Count.ToString();

		SetDeathZone();

		//si no se va a cargar la partida
		if(!ScenerySelection.LoadGame)
		{
			//choose turn
			var random=new Random();
			int currentTurn=random.Next(0,2);
			martianTurn=Convert.ToBoolean(currentTurn);
			if(martianTurn)
			{
				ShowMessage("¡Empiezan los marcianos!");
				Input.SetCustomMouseCursor(martianCursor, Input.CursorShape.Arrow, new Vector2(0,0));
				camera.Position=martiansCameraPosition;
			}
			else
			{
				ShowMessage("¡Empiezan los astronautas!");
				Input.SetCustomMouseCursor(astronautCursor, Input.CursorShape.Arrow, new Vector2(3,0));
				camera.Position=astronautsCameraPosition;
			}

			//reiniciar inventario
			Inventory.SelectedPlayer=null;
			Inventory.Unopenable=false;
			
		}
		else
		{
			LoadGame();			
		}

		//mobile
		if(Globals.MobileDevice)
		{
			GetNode<Node2D>("HUD/Zoom").Visible = false;
		}

	}

	
	public override void _Process(float delta)
	{
		zoomPercentage.Text=(200-(int)(camera.Zoom.x*100)).ToString()+"%";

		//ShowFPS
		//GetNode<Label>("HUD/FPS").Text=Engine.GetFramesPerSecond().ToString();
	}


	private void ShowMessage(string message)
	{
		messageTimer.Start();
		messageLabel.Text=message;
		messageLabel.Visible=true;
	}

	private void InitializeMembers()
	{
		//group astronauts and martians
		astronauts=GetNode("Astronauts").GetChildren();
		foreach(Jugador astronaut in astronauts)
		{
			astronaut.ToolsAvailable=new byte[9];
			Array.Copy(InventorySelection.AstronautsTools, astronaut.ToolsAvailable, 9);
		}

		martians=GetNode("Martians").GetChildren();
		foreach(Jugador martian in martians)
		{
			martian.AddToGroup("Martians");
			martian.IsMartian=true;

			//change animation
			AnimatedSprite animatedSprite=martian.GetNode<AnimatedSprite>("AnimatedSprite");
			animatedSprite.Animation="martian_idle";
			animatedSprite.Scale=new Vector2(0.369f, 0.366f);
			animatedSprite.Position=new Vector2(0, -3);

			//instanciar inventarios
			martian.ToolsAvailable=new byte[9];
			Array.Copy(InventorySelection.MartiansTools, martian.ToolsAvailable, 9);
		}	
	}

	private void SetDeathZone()
	{
		//death zone
		Area2D deathZone = GetNode<Area2D>("DeathZone");

		SegmentShape2D[] segments = new SegmentShape2D[4];
		const float adjustment=3000f;

		//segmento izquierdo
		segments[0] = new SegmentShape2D()
		{
			A = new Vector2(leftLimit, topLimit-adjustment), 
			B = new Vector2(leftLimit, bottomLimit)
		};
		//segmento superior
		segments[1] = new SegmentShape2D()
		{
			A = new Vector2(leftLimit, topLimit-adjustment),
			B = new Vector2(rightLimit, topLimit-adjustment)
		};

		//segmento derecho
		segments[2] = new SegmentShape2D()
		{
			A = new Vector2(rightLimit, topLimit-adjustment),
			B = new Vector2(rightLimit, bottomLimit)
		};

		//segmento inferior
		segments[3] = new SegmentShape2D()
		{
			A = new Vector2(rightLimit, bottomLimit),
			B = new Vector2(leftLimit, bottomLimit)
		};

		for (int i = 0; i < segments.Length; i++)
		{
			deathZone.GetChild<CollisionShape2D>(i).Shape = segments[i];
		}
	}

	private void _on_DeathZone_body_entered(Node body)
	{
		//GD.Print("body death");

		if(body is Jugador jugador)
		{
			signalManager.EmitSignal(nameof(General.OnPlayerDeath),jugador);
		}

		if(body is Teleporter teleporter)
		{
			body.QueueFree();
			signalManager.EmitSignal(nameof(General.OnTeleporterRemoved), teleporter);
			//EventManager.NotifyTeleporterRemoved(teleporter);
		}

		if(body is Iman iman)
		{
			//EventManager.OnTurnChanged-=iman.OnTurnChanged;
			signalManager.EmitSignal(nameof(General.OnMagnetRemoved), iman);
			iman.QueueFree();
		}

	}

	private void _on_DeathZone_area_entered(Node area)
	{
		if(area.Name.Equals("BananaIsColliding")) //plátano
		{
			//GetParent().QueueFree();
			ChangeTurn();
		}

		if(area is Thrower) return;


        if (area.GetParent() is not Throwable throwable) return;

        if (throwable is Iman) return;

		if(throwable is GloboTeledirigido globoTeledirigido)
		{
			signalManager.EmitSignal(nameof(General.OnRemoteBalloonRemoved), globoTeledirigido);
			ChangeTurn();
		}

		throwable.QueueFree();
	}

	private void OnPlayerDeath(Jugador jugador)
	{
		SubtractTeamNumber(1, jugador.IsMartian);
		if(Inventory.SelectedPlayer==jugador)
		{
			ChangeTurn();
		}

		deathSound.Play();
		jugador.QueueFree();
	}

	private void SubtractTeamNumber(byte subtrahend, bool martian)
	{
		Label label=martian ? martiansLabel : astronautsLabel;
		int num=Convert.ToInt32(label.Text);
		num=num-subtrahend;
		label.Text=num.ToString();	
		
		//GD.Print("time left: "+gameOverTimer.TimeLeft);
		//GD.Print("wait time: "+gameOverTimer.WaitTime);

		if(num<=0 && !gameOverTimerStarted)//&& gameOverTimer.TimeLeft<gameOverTimer.WaitTime)
		{
			gameOverTimer.Start();
			gameOverTimerStarted=true;
		}

	}
	

	private void _on_Timer_timeout()
    {
		messageLabel.Visible=false;
	}

	private void _on_GameStartSound_finished()
	{
		music.Play();
	}

	private void Reanudar()
	{
		GetNode<CanvasLayer>("HUD").Show();
	}

	public void ChangeInkVisibility(bool visible)
	{
		ink.Visible=visible;
	}

	protected void ChangeTurn()
	{
		AstronautsStars++;
		MartiansStars++;

		ink.Visible=false;

		if(!martianTurn)
		{
			ShowMessage("¡Turno de los marcianos!");
			Input.SetCustomMouseCursor(martianCursor, Input.CursorShape.Arrow, new Vector2(0,0));
		}
		else
		{
			ShowMessage("¡Turno de los astronautas!");
			Input.SetCustomMouseCursor(astronautCursor, Input.CursorShape.Arrow, new Vector2(3,0));
		}
		martianTurn=!martianTurn;
		EventManager.NotifyTurnChanged(martianTurn);
		signalManager.EmitSignal(nameof(General.OnTurnChanged), martianTurn);
		turnChangeSound.Play();
		
		
		//Specials

		if(martiansInvisible)
		{
			MartianTurnVisible();
			martiansInvisible=false;
		}

		astronautsSpecialTurnsLeft = (byte)Math.Max(astronautsSpecialTurnsLeft - 1, 0);
		astronautsSpecial.Visible = astronautsSpecialTurnsLeft <= 0;

		martiansSpecialTurnsLeft = (byte)Math.Max(martiansSpecialTurnsLeft - 1, 0);
		martiansSpecial.Visible = martiansSpecialTurnsLeft <= 0;

		astronautsSpecialActive=false;



		//inventario
		if(Inventory.SelectedPlayer!=null)
		{
			Inventory.SelectedPlayer.Moved=false;
		}
		
		Inventory.SelectedPlayer=null;
		Inventory.Unopenable=false;
		
	}


	public void AddStar(bool isMartian, bool changedTurn)
    {
        if(!changedTurn) //significa que la herramienta no cambió el turno al ser lanzada
        {
            if(isMartian==MartianTurn)
            {
                return;
            }


            if(MartianTurn)
            {
                MartiansStars++;
                GD.Print("se agregó una estrella a los marcianos");

				DisplayAddedStars(1, true);

                return;
            }

            AstronautsStars++;
            GD.Print("se agregó una estrella a los astronautas");
			DisplayAddedStars(1, false);


            return;
        }



        if(isMartian!=MartianTurn)
        {
            return;
        }

		DisplayAddedStars(1, !isMartian);

        if(!MartianTurn)
        {
            MartiansStars++;
            GD.Print("se agregó una estrella a los marcianos");
			DisplayAddedStars(1, true);

            return;
        }

        AstronautsStars++;
        GD.Print("se agregó una estrella a los astronautas");
		DisplayAddedStars(1, false);


    }

	public void DisplayAddedStars(int stars, bool martian)
	{
		Control addedStars= martian ? martiansAddedStars: astronautsAddedStars;
		addedStars.Visible=true;

		Label label=addedStars.GetNode<Label>("Label");
		label.Text=$"+{stars}";

		addedStarsTimer.Start();
	}

	private void DisplayAddedStarsTimeout()
	{
		martiansAddedStars.Visible=false;
		astronautsAddedStars.Visible=false;
	}


	private void _on_GameOverTimer_timeout()
	{
		if(astronautsLabel.Text.Equals("0") && martiansLabel.Text.Equals("0"))
		{
			GameOver(Constants.WinningTeam.Draw);
			return;
		}

		GameOver(astronautsLabel.Text=="0" ? Constants.WinningTeam.Martians : 
		Constants.WinningTeam.Astronauts);
	}

    private void GameOver(Constants.WinningTeam winningTeam) 
    {
		gameOverSound.Play();
		GetNode<CanvasLayer>("HUD").Visible=false;
		MatchEnding matchEnding=MatchEnding.GetMatchEnding(winningTeam);
		AddChild(matchEnding);
    }

}








