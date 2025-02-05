using Godot;

namespace FirstPersonController;

public partial class FirstPersonController : CharacterBody3D
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
		if (@event is InputEventMouseMotion eventMouseMotion)
		{
			Vector3 rotation = Camera.Rotation;
			Vector2 movement = eventMouseMotion.ScreenRelative * Sensitivity;
			Camera.Rotation = new Vector3(Mathf.Clamp(rotation.X - movement.Y, Mathf.DegToRad(-90), Mathf.DegToRad(90)), rotation.Y - movement.X, 0);
		}
	}

	public override void _PhysicsProcess(double delta)
	{
		Vector3 velocity = Velocity;
		
		if (!IsOnFloor())
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