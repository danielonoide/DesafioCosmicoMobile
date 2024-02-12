using Godot;
using System;


public class MainMenu : CanvasLayer
{
	readonly Vector2 movimiento=new(1,0);

	Camera2D camara;
	TextureButton start, opciones, exit, tutorialsButton;

    readonly Vector2 scaling = new(0.2f, 0.2f);
	
	Vector2 ogSettingsScale;
	
	public override void _Ready()
	{
		camara=GetNode<Camera2D>("Camera2D");
		start=GetNode<TextureButton>("Start");
		exit=GetNode<TextureButton>("Exit");		
		opciones=GetNode<TextureButton>("Opciones");
		tutorialsButton=GetNode<TextureButton>("TutorialsBTN");

        ogSettingsScale = opciones.RectScale;

		var buttons = GetTree().GetNodesInGroup("Buttons");

		foreach(TextureButton button in buttons)
		{
			button.Connect("mouse_entered", this, nameof(OnButtonMouseEntered), new Godot.Collections.Array{button});
			button.Connect("mouse_exited", this, nameof(OnButtonMouseExited), new Godot.Collections.Array{button});			
		}

		Input.SetCustomMouseCursor(null);
	}

	public override void _PhysicsProcess(float delta)
	{
		camara.Position+=movimiento;
	}

	private void OnButtonMouseEntered(TextureButton textureButton)  //señal 
	{
		textureButton.RectScale+=scaling;
	}

	private void OnButtonMouseExited(TextureButton textureButton) //señal
	{
		textureButton.RectScale-=scaling;
	}

	
	private void _on_Opciones_pressed()  //señal
	{
		AddChild(Settings.GetSettings());
		opciones.Hide();
	}

	private void _on_Tutorials_pressed() //señal
	{
		AddChild(Tutorials.GetTutorials());
	}

	private void CloseSettings() //señal
	{
		opciones.RectScale = ogSettingsScale;
		opciones.Show();
	}

	private void _on_Start_pressed() //señal
	{
		AddChild(ScenerySelection.GetScenerySelection());
	}

	private void _on_Exit_pressed() //señal
	{
		AddChild(AffirmationScreen.GetAffirmationScreen(AffirmationScreen.Actions.Quit, "¿Salir del juego?"));
	}


}













