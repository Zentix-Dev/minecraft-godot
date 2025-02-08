using Godot;
using Minecraft.scripts.worldgen;

namespace Minecraft;

public partial class EditorCameraSwitcher : Node3D
{
	[Export] public Node3D Player;
	[Export] public ChunkManager ChunkManager;
	[Export] public Control Crosshair;
	[Export] public PackedScene EditorCameraScene;

	private bool _editorCamEnabled;

	public void EnableEditorCamera()
	{
		Player.SetProcessMode(ProcessModeEnum.Disabled);
		
		Input.MouseMode = Input.MouseModeEnum.Visible;
		Crosshair.Hide();
		
		var editorCamParent = EditorCameraScene.Instantiate<Node3D>();
		AddChild(editorCamParent);
		
		var editorCam = editorCamParent.GetNode<Camera3D>("Camera3D");
		var playerCam = Player.GetNode<Camera3D>("Camera3D");

		editorCam.GlobalPosition = playerCam.GlobalPosition;
		editorCam.GlobalRotation = playerCam.GlobalRotation;
		editorCam.Current = true;

		ChunkManager.Viewer = editorCam;
	}

	public void DisableEditorCamera()
	{
		var editorCam = GetNode<Node3D>("EditorCamera");
		editorCam.QueueFree();
		
		Player.SetProcessMode(ProcessModeEnum.Inherit);
		
		Input.MouseMode = Input.MouseModeEnum.Captured;
		Crosshair.Show();

		ChunkManager.Viewer = Player;
	}

	public override void _Input(InputEvent @event)
	{
		if (@event is not InputEventKey { Pressed: true, Keycode: Key.F }) return;
		
		if (_editorCamEnabled)
			DisableEditorCamera();
		else
			EnableEditorCamera();
		_editorCamEnabled = !_editorCamEnabled;
	}
}