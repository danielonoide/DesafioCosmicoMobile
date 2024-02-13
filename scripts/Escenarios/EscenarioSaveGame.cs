using Godot;
using System;
using System.Collections.Generic;
using Newtonsoft.Json;


public partial class Escenario : Node2D
{
    
	protected Godot.Collections.Dictionary<string,object> SaveData()
	{
        Godot.Collections.Dictionary<string, object> saveData = new()
        {
            //zoom y posición de la camara
            { "CameraPosition", camera.Position },
            { "CameraZoom", camera.Zoom },
			{"InventoryUnopenable", Inventory.Unopenable},
			{"InventoryOpen", Inventory.Open}
        };

		if(Inventory.SelectedPlayer!=null)
		{
			saveData.Add("SelectedPlayer", Inventory.SelectedPlayer.GetPath());
		}

        //posiciones, inventarios y vida
		Godot.Collections.Array astronautsData=new();
        foreach (Jugador astronaut in astronauts)
		{
			if(IsInstanceValid(astronaut))
			{
				astronautsData.Add(astronaut.Save());
			}
			else
			{
				astronautsData.Add(null);
			}
		}

		saveData.Add("AstronautsData", astronautsData);

		Godot.Collections.Array martiansData=new();
		foreach(Jugador martian in martians)
		{
			if(IsInstanceValid(martian))
			{
				martiansData.Add(martian.Save());
			}
			else
			{
				martiansData.Add(null);
			}
		}

		saveData.Add("MartiansData", martiansData);

		//estrellas de los equipos
        saveData.Add("AstronautsStars", AstronautsStars);
        saveData.Add("MartiansStars", MartiansStars);

		//de quién es el turno
        saveData.Add("martianTurn", martianTurn);

		//habilidades especiales
        saveData.Add("astronautsSpecialTurnsLeft", astronautsSpecialTurnsLeft);
        saveData.Add("martiansSpecialTurnsLeft", martiansSpecialTurnsLeft);
		saveData.Add("astronautsSpecialActive", astronautsSpecialActive);

		saveData.Add("martiansInvisible", martiansInvisible);

		//herramientas
		var saveNodes=GetTree().GetNodesInGroup("Persist");
		Godot.Collections.Array nodesData=new();
		foreach(IPersist node in saveNodes)
		{
			var nodeData=node.Save();
			nodesData.Add(nodeData);			
		}
		if(nodesData.Count>0) saveData.Add("NodesData", nodesData);

		return saveData;
	}


	protected void SaveDictionary(Godot.Collections.Dictionary<string,object> saveData)
	{
		string saveFileName=Constants.SaveFileNames[(int)ScenerySelection.ActiveScenery];

		File saveGame=new();
		saveGame.Open(saveFileName, File.ModeFlags.Write);
		saveGame.StoreLine(JSON.Print(saveData));
		saveGame.Close();
	}


	public virtual void SaveGame()
	{
		SaveDictionary(SaveData());
	}

	public Godot.Collections.Dictionary<string, object> LoadDictionary()
	{
		string saveFileName=Constants.SaveFileNames[(int)ScenerySelection.ActiveScenery];
		File saveGame=new();
		saveGame.Open(saveFileName, File.ModeFlags.Read);
		Godot.Collections.Dictionary<string, object> saveData = new ((Godot.Collections.Dictionary)JSON.Parse(saveGame.GetLine()).Result);
		saveGame.Close();

		return saveData;
	}









	public virtual void LoadGame()
	{
		Godot.Collections.Dictionary<string,object> saveData=LoadDictionary();

		//cargar zoom y posición de la camara
		camera.Position=StringToVector2((string)saveData["CameraPosition"]);
		camera.Zoom=StringToVector2((string)saveData["CameraZoom"]);
		//Inventory.Unopenable=(bool)saveData["InventoryUnopenable"];
		

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

		//cargar información de los integrantes, inventarios y punto de humedad
		var astronautsData=(Godot.Collections.Array)saveData["AstronautsData"];
		LoadTeamInfo(astronautsData, astronauts, false);

		var martiansData=(Godot.Collections.Array)saveData["MartiansData"];
		LoadTeamInfo(martiansData, martians, true);

		//habilidad especial de los astronautas activa
		astronautsSpecialActive=(bool)saveData["astronautsSpecialActive"];

		if(astronautsSpecialActive)
		{
			AddAstronautsSpecial();
		}


		martiansInvisible=(bool)saveData["martiansInvisible"];


		//jugador seleccionado
		if(saveData.ContainsKey("SelectedPlayer"))
		{
			string nodePath=saveData["SelectedPlayer"].ToString();
			Inventory.SelectedPlayer=GetNode<Jugador>(nodePath);
		}
		else
		{
			Inventory.SelectedPlayer=null;
		}
		Inventory.Unopenable=(bool)saveData["InventoryUnopenable"];
		bool inventoryOpen=(bool)saveData["InventoryOpen"];
		if(inventoryOpen)
		{
			Inventory.Unopenable=false;
		}

		//herramientas
		if(!saveData.ContainsKey("NodesData")) return;

		byte? cancelAction=null;

		var nodeData=(Godot.Collections.Array)saveData["NodesData"];
		foreach(Godot.Collections.Dictionary node in nodeData)
		{
			if(node is null) continue;

			var newObjectScene = (PackedScene)ResourceLoader.Load(node["Filename"].ToString());
        	var newObject = (Node2D)newObjectScene.Instance();

			if(newObject is Lanzaglobos lanzaglobos)
			{
				lanzaglobos.balloonsLaunched=Convert.ToByte(node["balloonsLaunched"]);
				GetNode(node["Parent"].ToString()).AddChild(lanzaglobos);

				if(lanzaglobos.balloonsLaunched<1)
				{
					cancelAction=Convert.ToByte(Array.IndexOf(Inventory.toolNames, lanzaglobos.GetType().Name));
				}
				continue;
			}

			newObject.Position=StringToVector2((string)node["Position"]);
			Vector2 velocity=Vector2.Zero;
			if(newObject is Throwable throwable)
			{
				velocity=StringToVector2((string)node["velocity"]);
				throwable.SetVelocity(StringToVector2((string)node["velocity"]));
			}


			AddChild(newObject);

			if(newObject is Iman iman)
			{
				iman.martianLaunched=(bool)node["martianLaunched"]; //tiene que ponerse después porque se modifica en el _Ready()
				iman.turns=Convert.ToInt32(node["turns"]);
				iman.launched=(bool)node["launched"];
				iman.playerDetector.Monitoring=iman.launched;

				if(!iman.launched)
				{
					cancelAction=Convert.ToByte(Array.IndexOf(Inventory.toolNames, iman.GetType().Name));
				}
			}

			if(newObject is Platano platano)
			{
				platano.martianDropped=(bool)node["martianDropped"];
				platano.dropped=(bool)node["dropped"];
				platano.loaded=velocity.y<5 && platano.dropped; //GUARRADA
				platano.launchButton.Visible = !platano.dropped;
				platano.detectPlayers=platano.dropped;

				platano.collisionShape2D.Disabled=!platano.dropped;

				if(!platano.dropped)
				{
					cancelAction=Convert.ToByte(Array.IndexOf(Inventory.toolNames, platano.GetType().Name));
				}
			}

			if(newObject is GloboTeledirigido globoTeledirigido)
			{
				cancelAction=Convert.ToByte(Array.IndexOf(Inventory.toolNames, globoTeledirigido.GetType().Name));
			}

			if(newObject is Throwable throwable1 && newObject is not Platano &&
			newObject is not GloboTeledirigido && velocity==Vector2.Zero)
			{
				if(throwable1 is Iman iman1 && iman1.launched) continue;
				Thrower thrower=Thrower.GetThrower(throwable1, throwable1.MaxSize);
				throwable1.AddChild(thrower);

				cancelAction=Convert.ToByte(Array.IndexOf(Inventory.toolNames, throwable1.GetType().Name));
			}

		}

		if(cancelAction!=null)
		{
			AddChild(ActionCanceller.GetToolCanceller((byte)cancelAction));
		}
		
	}

	protected Vector2 StringToVector2(string vectorString)
	{
		// eliminar los paréntesis y dividir el string en dos partes
		string[] parts = vectorString.Replace("(", "").Replace(")", "").Split(',');

		float x = Convert.ToSingle(parts[0]);
		float y = Convert.ToSingle(parts[1]);

		Vector2 vector2 = new(x, y);
		return vector2;
	}

	private void LoadTeamInfo(Godot.Collections.Array mapArray, Godot.Collections.Array teamArray, bool martianTeam)
	{
		for(int i=0; i<teamArray.Count; i++)
		{
			Jugador jugador=(Jugador)teamArray[i];

			if(mapArray[i] is null)
			{
				jugador.QueueFree();
				SubtractTeamNumber(1, martianTeam);
				continue;
			}

			Godot.Collections.Dictionary map=(Godot.Collections.Dictionary)mapArray[i];

			jugador.Position=StringToVector2((string)map["Position"]);

			jugador.ToolsAvailable=
			JsonConvert.DeserializeObject<byte[]>(map["ToolsAvailable"].ToString());

			jugador.HumidityPoints=Convert.ToByte(map["HumidityPoints"]);
			jugador.AddHumidity(0);

			Vector2 velocity=StringToVector2((string)map["velocity"]);
			jugador.SetVelocity(velocity);

			jugador.Moved=(bool)map["Moved"];
			if(map.Contains("Teleporter"))
			{
				var keyValuePairs=(Godot.Collections.Dictionary)map["Teleporter"];

				var newObjectScene=(PackedScene)ResourceLoader.Load(keyValuePairs["Filename"].ToString());
				Teleporter teleporter=(Teleporter)newObjectScene.Instance();
				teleporter.Position=StringToVector2((string)keyValuePairs["Position"]);
				teleporter.SetVelocity(StringToVector2((string)keyValuePairs["velocity"]));
				AddChild(teleporter);
				jugador.ActiveTeleporter=teleporter;

				bool teleporterLaunched=(bool)keyValuePairs["launched"];
				if(!teleporterLaunched)
				{
					Thrower thrower=Thrower.GetThrower(teleporter, teleporter.MaxSize);
					teleporter.AddChild(thrower);
					AddChild(ActionCanceller.GetToolCanceller(Convert.ToByte(Array.IndexOf(Inventory.toolNames, teleporter.GetType().Name))));
				}
			}

			//si estaba por moverse
			jugador.BoutaMove=(bool)map["BoutaMove"];
			if(jugador.BoutaMove)
			{
				Thrower thrower=Thrower.GetThrower(jugador, jugador.MaxSize);
				jugador.AddChild(thrower);

				AddChild(ActionCanceller.GetMovementCanceller());
			}

		}

	}

}