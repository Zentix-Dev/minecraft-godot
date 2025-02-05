using Godot;

namespace Minecraft.scripts.game;

public partial class PlayerController : CharacterBody3D
{
	[Export, ExportGroup("Camera")] 
	public Node3D Camera;
	[Export]
	public Vector2 Sensitivity = Vector2.One;
	
	[Export, ExportGroup("Movement")]
	public float Speed = 5.0f;
	[Export]
	public float JumpVelocity = 6;

	[Export] public float Weight = 2;

	private bool _gravityEnabled = false;
	private bool _inputEnabled = false;

	private bool _escaped;

	public override void _EnterTree()
	{
		Input.SetMouseMode(Input.MouseModeEnum.Captured);
	}

	public override void _ExitTree()
	{
		Input.SetMouseMode(Input.MouseModeEnum.Visible);
	}

	public override void _UnhandledInput(InputEvent @event)
	{
		if (@event is InputEventKey keyEvent)
		{
			if (keyEvent.Pressed && keyEvent.Keycode == Key.Escape)
			{
				Input.SetMouseMode(Input.MouseModeEnum.Visible);
				_escaped = true;
			}
		}

		if (@event is InputEventMouseButton mouseEvent)
		{
			if (mouseEvent.Pressed && _escaped)
			{
				_escaped = false;
				Input.SetMouseMode(Input.MouseModeEnum.Captured);
			}
		}
		
		if (!_inputEnabled || _escaped) return;
		if (@event is InputEventMouseMotion eventMouseMotion)
		{
			Vector3 rotation = Camera.Rotation;
			Vector2 movement = eventMouseMotion.ScreenRelative * Sensitivity;
			Camera.Rotation = new Vector3(Mathf.Clamp(rotation.X - movement.Y, Mathf.DegToRad(-90), Mathf.DegToRad(90)), rotation.Y - movement.X, 0);
		}
	}

	public void OnChunksLoaded()
	{
		_gravityEnabled = true;
		_inputEnabled = true;
	}

	public override void _PhysicsProcess(double delta)
	{
		Vector3 velocity = Velocity;
		
		if (!IsOnFloor() && _gravityEnabled)
		{
			velocity += GetGravity() * Weight * (float)delta;
		}
		
		if (Input.IsActionPressed("jump") && IsOnFloor())
		{
			velocity.Y = JumpVelocity;
		}
		
		Vector2 inputDir = Input.GetVector("left", "right", "up", "down");
		Vector3 direction = new Vector3(inputDir.X, 0, inputDir.Y).Rotated(Vector3.Up, Camera.Rotation.Y).Normalized();
		if (direction != Vector3.Zero)
		{
			velocity.X = direction.X * Speed;
			velocity.Z = direction.Z * Speed;
		}
		else
		{
			velocity.X = Mathf.MoveToward(Velocity.X, 0, Speed);
			velocity.Z = Mathf.MoveToward(Velocity.Z, 0, Speed);
		}

		Velocity = velocity;
		MoveAndSlide();
	}
}