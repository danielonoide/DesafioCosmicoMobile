using Godot;
using System;
using System.Collections.Generic;
using Newtonsoft.Json;


public class Escenario : Node2D
{
	protected Camera2D camera;
	protected Label zoomPercentage;
	protected float zoom=0.1f; //los saltos
	protected bool rightClick=false;
	protected const float maxZoom=0.5f;
	protected const float minZoom=1.9f;

	protected const float realMinZoom=1.95f;
	protected Vector2 cameraSize=new Vector2(1366, 768);
	protected float leftLimit=-2500f;  //a los límites de la cámara le restamos la mitad de su ancho
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

	bool martiansInvisible=false;

	Timer messageTimer;
	Timer gameOverTimer;
	Label messageLabel;

	Texture astronautCursor=GD.Load<Texture>("res://sprites/cursors/spaceship3.png");
	Texture martianCursor=GD.Load<Texture>("res://sprites/cursors/alien_cursor4.png");

	Texture astronautTexture=GD.Load<Texture>("res://sprites/characters/astronaut_idle_single.png");
	Texture martianTexture=GD.Load<Texture>("res://sprites/characters/martian_idle.png");

	AudioStreamPlayer gameOverSound;
	AudioStreamPlayer turnChangeSound;

	AudioStreamPlayer deathSound;

	protected Vector2 astronautsCameraPosition=new Vector2(0,0);
	protected Vector2 martiansCameraPosition=new Vector2(0,0);


	Godot.Collections.Array martians, astronauts;

	Label martiansLabel, astronautsLabel;

	TextureRect ink;

	static bool playerDeathEventSuscribed=false;


	General signalManager;

	
	public override void _Ready()
	{
		//signals
		signalManager=GetNode<General>("/root/General");
		signalManager.Connect(nameof(General.OnPlayerDeath), this, nameof(OnPlayerDeath));


		//PauseButton.GetPauseButton().Connect("BotonPausaPresionado", this, nameof(BotonPausaPresionado)); //nombre de la señal, objetivo y funcion a ejecutar
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
		//matchSFX["GameStart"]=GetNode<AudioStreamPlayer>("MatchSFX/GameStart");
		gameOverSound=GetNode<AudioStreamPlayer>("MatchSFX/GameOver");
		turnChangeSound=GetNode<AudioStreamPlayer>("MatchSFX/TurnChange");
		deathSound=GetNode<AudioStreamPlayer>("DeathSound");

		//group astronauts and martians
		astronauts=GetNode("Astronauts").GetChildren();
		foreach(Jugador astronaut in astronauts)
		{
			astronaut.AddToGroup("Astronauts");
			astronaut.ToolsAvailable=new byte[9];
			Array.Copy(InventorySelection.AstronautsTools, astronaut.ToolsAvailable, 9);

			//astronaut.AddChild(Inventory.GetInventory(InventorySelection.astronautsTools));
		}

		martians=GetNode("Martians").GetChildren();
		foreach(Jugador martian in martians)
		{
			martian.AddToGroup("Martians");
			martian.IsMartian=true;
			//change sprite
/* 			Sprite sprite=martian.GetNode<Sprite>("Sprite");
			sprite.Texture=martianTexture;
			sprite.Hframes=1;
			sprite.Vframes=1;
			sprite.Scale=new Vector2(0.369f, 0.366f); */

			//change animation
			AnimatedSprite animatedSprite=martian.GetNode<AnimatedSprite>("AnimatedSprite");
			animatedSprite.Animation="martian_idle";
			animatedSprite.Scale=new Vector2(0.369f, 0.366f);
			animatedSprite.Position=new Vector2(0, -3);

			//instanciar inventarios
			martian.ToolsAvailable=new byte[9];
			Array.Copy(InventorySelection.MartiansTools, martian.ToolsAvailable, 9);
			//martian.AddChild(Inventory.GetInventory(InventorySelection.martiansTools));

		}		

		//initialize stars
		AstronautsStars=0;
		MartiansStars=0;

		//team counters
		astronautsLabel=GetNode<Label>("HUD/TeamInfo/AstronautsCounter");
		martiansLabel=GetNode<Label>("HUD/TeamInfo/MartiansCounter");

		astronautsLabel.Text=astronauts.Count.ToString();
		martiansLabel.Text=martians.Count.ToString();


		//death zone
		Area2D deathZone = GetNode<Area2D>("DeathZone");

		SegmentShape2D[] segments = new SegmentShape2D[3];

		segments[0] = new SegmentShape2D()
		{
			A = new Vector2(leftLimit, topLimit),
			B = new Vector2(leftLimit, bottomLimit)
		};

/* 		segments[1] = new SegmentShape2D()
		{
			A = new Vector2(leftLimit, topLimit),
			B = new Vector2(rightLimit, topLimit)
		}; */

		segments[1] = new SegmentShape2D()
		{
			A = new Vector2(rightLimit, topLimit),
			B = new Vector2(rightLimit, bottomLimit)
		};

		segments[2] = new SegmentShape2D()
		{
			A = new Vector2(rightLimit, bottomLimit),
			B = new Vector2(leftLimit, bottomLimit)
		};

		for (int i = 0; i < segments.Length; i++)
		{
			deathZone.GetChild<CollisionShape2D>(i).Shape = segments[i];
		}

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
			
			
		}
		else
		{
			LoadGame();			
		}

		//reiniciar inventario
		Inventory.SelectedPlayer=null;
		Inventory.Unopenable=false;

	}

	
	public override void _Process(float delta)
	{
		zoomPercentage.Text=(200-(int)(camera.Zoom.x*100)).ToString()+"%";

		//ShowFPS
		GetNode<Label>("HUD/FPS").Text=Engine.GetFramesPerSecond().ToString();
	}
	
	private void ShowMessage(string message)
	{
		messageTimer.Start();
		messageLabel.Text=message;
		messageLabel.Visible=true;
	}

	private void _on_Timer_timeout()
    {
		messageLabel.Visible=false;
	}

	private void _on_GameStartSound_finished()
	{
		music.Play();
	}

	public override void _UnhandledInput(InputEvent @event)
	{
		if(@event is InputEventMouseButton evento)
		{
			if (evento.Pressed && evento.ButtonIndex==(int)ButtonList.WheelDown) 
			{
				UnZoom();
			}
			if (evento.Pressed && evento.ButtonIndex==(int)ButtonList.WheelUp)
			{
				Zoom();		
			}	
			
			if(evento.ButtonIndex==2)
			{
				if(evento.Pressed)
				{
					rightClick=true;
				}
				else
				{
					rightClick=false;
				}
			}
		}
		
		if(@event is InputEventMouseMotion Movimiento)
		{
			if (rightClick)
			{
				camera.SmoothingEnabled = false;
				Vector2 newPosition = camera.Position - Movimiento.Relative * camera.Zoom;
				float leftLimitZoom=leftLimit+( (cameraSize.x*camera.Zoom.x)/2 );
				float rightLimitZoom=rightLimit-( (cameraSize.x*camera.Zoom.x)/2 );
				float topLimitZoom=topLimit+( (cameraSize.y*camera.Zoom.y)/2 );
				float bottomLimitZoom=bottomLimit-( (cameraSize.y*camera.Zoom.y)/2 );

				// Verificar límites de la cámara
				newPosition.x = Mathf.Clamp(newPosition.x, leftLimitZoom, rightLimitZoom);
				newPosition.y = Mathf.Clamp(newPosition.y, topLimitZoom, bottomLimitZoom);

				camera.Position=newPosition;
			}
			else
			{
				camera.SmoothingEnabled = true;
			}
		}
		
	}
	
	protected void _on_Zoom_pressed()
	{
		Zoom();
	}


	protected void _on_UnZoom_pressed()
	{
		UnZoom();
	}

	void Zoom()
	{
		if(camera.Zoom.x>maxZoom)
		{
			float newZoom=(float)Math.Round(camera.Zoom.x-zoom,1);
			camera.Zoom=new Vector2(newZoom, newZoom); 	
		}
	}

	void UnZoom()
	{
		if(camera.Zoom.x<minZoom)
		{
			float newZoom=(float)Math.Round(camera.Zoom.x+zoom,1);
			camera.Zoom=new Vector2(newZoom, newZoom); 	
			return;
		}

		if(camera.Zoom.x==minZoom)
		{
			camera.Zoom=new Vector2(realMinZoom, realMinZoom); 	
		}
	}

	private void Reanudar()
	{
		//AddChild(PauseButton.GetPauseButton());
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
		
	}


	private void _on_AstronautSpecial_pressed()
	{
		if(martianTurn) return;

		//si el jugador se movió
		if(Inventory.SelectedPlayer!=null)
		{
			Inventory.SelectedPlayer.Moved=false;
			if(!Inventory.SelectedPlayer.IsOnFloor()) return;
		}
		
		astronautsSpecialTurnsLeft=3;
		astronautsSpecial.Visible=false;
		AstronautsSpecial astronautShip=AstronautsSpecial.GetAstronautsSpecial();
		astronautShip.Position=new Vector2(GetGlobalMousePosition().x, topLimit);
		AddChild(astronautShip);


	}

	private void _on_MartianSpecial_pressed()
	{
		if(!martianTurn) return;
		//si el jugador se movió
		if(Inventory.SelectedPlayer!=null)
		{
			Inventory.SelectedPlayer.Moved=false;
		}

		martiansSpecialTurnsLeft=3;
		martiansSpecial.Visible=false;
		MartianTurnInvisible();
		ChangeTurn();
		martiansInvisible=true;

	}

	private void MartianTurnInvisible()
	{
		foreach(Jugador martian in martians)
		{
			if(IsInstanceValid(martian))
			{
				martian.Visible=false;
			}
		}
	}

	private void MartianTurnVisible()
	{
		foreach(Jugador martian in martians)
		{
			if(IsInstanceValid(martian))
			{
				martian.Visible=true;
			}
		}
	}


	public static void AddStar(bool isMartian, bool changedTurn)
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
                return;
            }

            AstronautsStars++;
            GD.Print("se agregó una estrella a los astronautas");

            return;
        }



        if(isMartian!=Escenario.MartianTurn)
        {
            return;
        }

        if(!MartianTurn)
        {
            MartiansStars++;
            GD.Print("se agregó una estrella a los marcianos");
            return;
        }

        AstronautsStars++;
        GD.Print("se agregó una estrella a los astronautas");
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
			iman.QueueFree();
		}

	}

	private void _on_DeathZone_area_entered(Node area)
	{
		if(area.Name.Equals("BananaIsColliding")) //plátano
		{
			//GetParent().QueueFree();
			ChangeTurn();
			return; //tiene 2 area2d
		}

		Throwable throwable = area.GetParent() as Throwable;
		if (throwable == null) return;

		throwable.QueueFree();

/* 		if (GetTree().HasGroup("Lanzaglobos"))
		{
			Lanzaglobos lanzaglobos = GetTree().GetNodesInGroup("Lanzaglobos")[0] as Lanzaglobos;
			lanzaglobos.BalloonsExploded++;
			if (lanzaglobos != null && lanzaglobos.BalloonsExploded == 3)
			{
				ChangeTurn();
				lanzaglobos.QueueFree();
			}
			return;

		} */

		//ChangeTurn();

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


/* 		if(!jugador.IsQueuedForDeletion())
		{
			jugador.QueueFree();
		} */

	}

	private void SubtractTeamNumber(byte subtrahend, bool martian)
	{
		Label label=martian ? martiansLabel : astronautsLabel;
		int num=Convert.ToInt32(label.Text);
		num=num-subtrahend;
		label.Text=num.ToString();	

		if(num<=0 && gameOverTimer.TimeLeft<gameOverTimer.WaitTime)
		{
			gameOverTimer.Start();
		}

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


	public void SaveGame()
	{
		string saveFileName=Constants.SaveFileNames[(int)ScenerySelection.ActiveScenery];

		Godot.Collections.Dictionary<string, object> saveData = new();

		//posiciones, inventarios y vida
		foreach(Jugador astronaut in astronauts)
		{
			if(IsInstanceValid(astronaut))
			{
				saveData.Add(astronaut.Name+"AstronautPosition", astronaut.Position);
				saveData.Add(astronaut.Name+"AstronautTools", astronaut.ToolsAvailable);
				saveData.Add(astronaut.Name+"AstronautPoints", astronaut.HumidityPoints);	
			}
		}

		foreach(Jugador martian in martians)
		{
			if(IsInstanceValid(martian))
			{
				saveData.Add(martian.Name+"MartianPosition", martian.Position);
				saveData.Add(martian.Name+"MartianTools", martian.ToolsAvailable);
				saveData.Add(martian.Name+"MartianPoints", martian.HumidityPoints);
			}
		}

		//estrellas de los equipos
        saveData.Add("AstronautsStars", AstronautsStars);
        saveData.Add("MartiansStars", MartiansStars);

		//de quién es el turno
        saveData.Add("martianTurn", martianTurn);

		//habilidades especiales
        saveData.Add("astronautsSpecialTurnsLeft", astronautsSpecialTurnsLeft);
        saveData.Add("martiansSpecialTurnsLeft", martiansSpecialTurnsLeft);


		//plátanos
		if(GetTree().HasGroup("Bananas"))
		{
/* 			var bananas=GetTree().GetNodesInGroup("Bananas");
			List<PlatanoData> platanos=new();
			foreach(Platano banana in bananas)
			{
				PlatanoData platano=new(banana);				
				platanos.Add(platano);
			}
			string platanosJson=JsonConvert.SerializeObject(platanos);
			GD.Print(platanosJson);
			saveData.Add("Bananas", platanos); */

			var bananas = GetTree().GetNodesInGroup("Bananas");
			Godot.Collections.Array bananasData = new();
			foreach (Platano banana in bananas)
			{
				Godot.Collections.Dictionary bananaData = new()
				{
					{"Position", banana.Position },
					{"martianDropped", banana.martianDropped},
					{"velocity", banana.GetVelocity()}
				};
				bananasData.Add(bananaData);
			}
			saveData.Add("Bananas", bananasData);
		}

		if(GetTree().HasGroup("Magnets"))
		{
/* 			var magnets=GetTree().GetNodesInGroup("Magnets");
			List<ImanData> imanes=new();
			foreach(Iman magnet in magnets)
			{
				ImanData iman=new(magnet);
				imanes.Add(iman);
			}
			string imanesJson=JsonConvert.SerializeObject(imanes);
			GD.Print(imanesJson);
			saveData.Add("Magnets", imanes) */;

			var magnets = GetTree().GetNodesInGroup("Magnets");
			Godot.Collections.Array magnetsData = new();
			foreach (Iman magnet in magnets)
			{
				Godot.Collections.Dictionary<string, object> magnetData = new() 
				{
					{ "Position", magnet.Position },
					{"martianLaunched", magnet.martianLaunched},
					{"turns", magnet.turns},
					{"velocity", magnet.GetVelocity()}
				};
				magnetsData.Add(magnetData);
			}
			saveData.Add("Magnets", magnetsData);
		}

		//guardar todo
		File saveGame=new();
		saveGame.Open(saveFileName, File.ModeFlags.Write);
		saveGame.StoreLine(JSON.Print(saveData));
		saveGame.Close();
	}











	public void LoadGame()
	{
		string saveFileName=Constants.SaveFileNames[(int)ScenerySelection.ActiveScenery];

		File saveGame=new();
		saveGame.Open(saveFileName, File.ModeFlags.Read);
		Godot.Collections.Dictionary<string, object> saveData = new Godot.Collections.Dictionary<string, object>((Godot.Collections.Dictionary)JSON.Parse(saveGame.GetLine()).Result);
		saveGame.Close();

		//cargar posiciones, inventarios y punto de humedad
		foreach(Jugador astronaut in astronauts)
		{
			if(saveData.ContainsKey(astronaut.Name+"AstronautPosition"))
			{
				astronaut.Position=StringToVector2(saveData[astronaut.Name+"AstronautPosition"].ToString());
				//astronaut.ToolsAvailable=(byte[])saveData[astronaut.Name+"AstronautTools"]; //Array.Copy
				
				GD.Print(saveData[astronaut.Name+"AstronautTools"].ToString());
				astronaut.ToolsAvailable=
				JsonConvert.DeserializeObject<byte[]>(saveData[astronaut.Name+"AstronautTools"].ToString());

				astronaut.HumidityPoints=Convert.ToByte(saveData[astronaut.Name+"AstronautPoints"]);
				astronaut.AddHumidity(0);
			}
			else
			{
				astronaut.QueueFree();
				SubtractTeamNumber(1,false);
			}
		}

		foreach(Jugador  martian in martians)
		{
			if(saveData.ContainsKey(martian.Name+"MartianPosition"))
			{
				martian.Position=StringToVector2(saveData[martian.Name+"MartianPosition"].ToString());
				martian.ToolsAvailable=
				JsonConvert.DeserializeObject<byte[]>(saveData[martian.Name+"MartianTools"].ToString());

				martian.HumidityPoints=Convert.ToByte(saveData[martian.Name+"MartianPoints"]);
				martian.AddHumidity(0);
			}
			else
			{
				martian.QueueFree();
				SubtractTeamNumber(1,true);
			}
		}

		//cargar estrellas
		AstronautsStars=Convert.ToInt32(saveData["AstronautsStars"]);
		MartiansStars=Convert.ToInt32(saveData["MartiansStars"]);

		AstronautsStars--;
		MartiansStars--;

		//cargar especiales
		astronautsSpecialTurnsLeft=Convert.ToByte(saveData["astronautsSpecialTurnsLeft"]);
		martiansSpecialTurnsLeft=Convert.ToByte(saveData["martiansSpecialTurnsLeft"]);

		astronautsSpecialTurnsLeft++;
		martiansSpecialTurnsLeft++;

		//turno
		martianTurn=(bool)saveData["martianTurn"];
		//cursor y todo ese pedo
		martianTurn=!martianTurn;
		ChangeTurn();



		//bananas
/* 		if(saveData.ContainsKey("Bananas"))
		{
			List<PlatanoData> bananas=JsonConvert.DeserializeObject<List<PlatanoData>>((string)saveData["Bananas"]);
			GD.Print("lista");
			foreach(var banana in bananas)
			{
				Platano platano=Platano.GetPlatano(banana);
				AddChild(platano);
			}
			GD.Print("foreach");
		}

		//magnets
		if(saveData.ContainsKey("Magnets"))
		{
			List<ImanData> magnets=JsonConvert.DeserializeObject<List<ImanData>>((string)saveData["Magnets"]);
			GD.Print("lista");
			foreach(var magnet in magnets)
			{
				Iman iman=Iman.GetIman(magnet);
				AddChild(iman);
			}
			GD.Print("foreach");
		} */

		if (saveData.ContainsKey("Bananas"))
		{
			Godot.Collections.Array bananasData = (Godot.Collections.Array)saveData["Bananas"];

			foreach (Godot.Collections.Dictionary bananaData in bananasData)
			{
				Vector2 bananaPosition = StringToVector2((string)bananaData["Position"]);
				Platano platano = Platano.GetPlatano();
				platano.martianDropped=(bool)bananaData["martianDropped"];
				platano.Position = bananaPosition;
				platano.loaded=true;
				AddChild(platano);

				
				Vector2 bananaVelocity=StringToVector2((string)bananaData["velocity"]);
				platano.SetVelocity(bananaVelocity);
			}
		}

		// Cargar imanes
		if (saveData.ContainsKey("Magnets"))
		{
			Godot.Collections.Array magnetsData = (Godot.Collections.Array)saveData["Magnets"];
			foreach (Godot.Collections.Dictionary magnetData in magnetsData)
			{
				Vector2 magnetPosition = StringToVector2((string)magnetData["Position"]);
				Iman iman = Iman.GetIman();
				iman.Position = magnetPosition;
				iman.turns=Convert.ToInt32(magnetData["turns"]);
				AddChild(iman);

				iman.martianLaunched=(bool)magnetData["martianLaunched"];
				Vector2 magnetVelocity=StringToVector2((string)magnetData["velocity"]);
				iman.SetVelocity(magnetVelocity);
			}
		}
	
	}

	private Vector2 StringToVector2(string vectorString)
	{
		// Eliminar los paréntesis y dividir el string en dos partes
		string[] parts = vectorString.Replace("(", "").Replace(")", "").Split(',');

		// Convertir cada parte en un valor numérico
		float x = Convert.ToSingle(parts[0]);
		float y = Convert.ToSingle(parts[1]);

		// Crear el objeto Vector2
		Vector2 vector2 = new Vector2(x, y);
		return vector2;
	}

/* 	public static Vector2 StringToVector2(string vectorString)
	{
		// Eliminar los caracteres "{" y "}" del string
		vectorString = vectorString.Replace("{", "").Replace("}", "");

		// Separar los valores por las comas
		string[] vectorComponents = vectorString.Split(',');

		// Convertir los strings a valores numéricos flotantes
		float x = float.Parse(vectorComponents[0].Split(':')[1]);
		float y = float.Parse(vectorComponents[1].Split(':')[1]);

		// Crear el objeto Vector2 con los valores obtenidos
		return new Vector2(x, y);
	}
 */




}








