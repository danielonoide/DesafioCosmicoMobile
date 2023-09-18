using Godot;
using System;


public class MainMenu : CanvasLayer
{
	readonly Vector2 movimiento=new(1,0);

	Camera2D camara;
	TextureButton start, opciones, exit, tutorialsButton;

    readonly Vector2 scaling = new(0.2f, 0.2f);
	
	
	public override void _Ready()
	{
		camara=GetNode<Camera2D>("Camera2D");
		start=GetNode<TextureButton>("Start");
		exit=GetNode<TextureButton>("Exit");		
		opciones=GetNode<TextureButton>("Opciones");
		tutorialsButton=GetNode<TextureButton>("TutorialsBTN");

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

	private void OnButtonMouseEntered(TextureButton textureButton)
	{
		textureButton.RectScale+=scaling;
	}

	private void OnButtonMouseExited(TextureButton textureButton)
	{
		textureButton.RectScale-=scaling;
	}

	
	private void _on_Opciones_pressed()
	{
		AddChild(Settings.GetSettings());
		opciones.Hide();
	}

	private void _on_Tutorials_pressed()
	{
		AddChild(Tutorials.GetTutorials());
	}

	private void CloseSettings()
	{
		opciones.RectScale-=scaling;
		opciones.Show();
	}

	private void _on_Start_pressed()
	{
		AddChild(ScenerySelection.GetScenerySelection());
	}

	private void _on_Exit_pressed()
	{
		AddChild(AffirmationScreen.GetAffirmationScreen(AffirmationScreen.Actions.Quit, "¿Salir del juego?"));
	}


}













