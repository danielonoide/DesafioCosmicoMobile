using Godot;

public class Settings : CanvasLayer
{
    HSlider[] volumeSliders = new HSlider[3];
    Label[] volumeLabels = new Label[3];
    CheckButton fullscreenToggle;

    General signalManager;

    public override void _Ready()
    {
        signalManager=GetNode<General>("/root/General");
        signalManager.Connect(nameof(General.OnScreenStatusChanged), this, nameof(OnScreenStatusChanged));

        var sliderNodes = GetTree().GetNodesInGroup("HSlider");
        for (int i = 0; i < volumeSliders.Length; i++)
        {
            volumeSliders[i] = (HSlider)sliderNodes[i];
            volumeSliders[i].Connect("value_changed", this, nameof(OnVolumeSliderValueChanged), new Godot.Collections.Array { i });

            volumeLabels[i] = volumeSliders[i].GetNode<Label>("Label");
            volumeLabels[i].Text = (Volume.Volumes[i] * 100).ToString();
            volumeSliders[i].Value = Volume.Volumes[i] * 100;
        }

        fullscreenToggle = GetNode<CheckButton>("VBoxContainer/HBoxContainer/CheckButton");
        fullscreenToggle.Pressed = OS.WindowFullscreen;

        if(Globals.MobileDevice)
        {
            GetNode<Container>("%HBoxContainer").Visible = false;
        }
    }

    private void OnScreenStatusChanged(bool fullscreen)
    {
        fullscreenToggle.Pressed=fullscreen;
    }

    private void _on_CheckButton_toggled(bool buttonPressed)
    {
        OS.WindowFullscreen = buttonPressed;
    }

    private void _on_Close_pressed()
    {
        QueueFree();
        GetTree().CallGroup("Menus", "CloseSettings");
    }

    private void OnVolumeSliderValueChanged(float value, int nodeIndex)
    {
        AudioServer.SetBusVolumeDb(nodeIndex, GD.Linear2Db(value / 100));
        Volume.Volumes[nodeIndex] = value / 100;
        volumeLabels[nodeIndex].Text = value.ToString();
    }

	public static CanvasLayer GetSettings()
    {
        PackedScene menuSettings = (PackedScene)ResourceLoader.Load("res://scenes/UI/Settings.tscn");
        return (CanvasLayer)menuSettings.Instance();
    }

}


