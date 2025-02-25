using Godot;
using Minecraft.scripts.engine;
using Minecraft.scripts.worldgen;

namespace Minecraft.scripts.game;

public partial class PlayerController : CharacterBody3D
{
	[Export] private ChunkManager _chunkManager;
	
	[Export, ExportGroup("Camera")] 
	public Node3D Camera;
	[Export]
	public Vector2 Sensitivity = Vector2.One;
	
	[Export, ExportGroup("Movement")]
	public float Speed = 5.0f;
	[Export] public float JumpVelocity = 6;
	[Export] public float Weight = 2;
	[Export] private float _waterDrag;
	[Export] private float _waterJumpVelocity;

	private bool _gravityEnabled = false;
	private bool _inputEnabled = false;

	private bool _escaped;
	private bool _isInWater;

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
				GetViewport().SetInputAsHandled();
				Input.SetMouseMode(Input.MouseModeEnum.Visible);
				_escaped = true;
			}
		}

		if (@event is InputEventMouseButton mouseEvent)
		{
			if (mouseEvent.Pressed && _escaped)
			{
				GetViewport().SetInputAsHandled();
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

		Vector2I chunkPos = ChunkManager.Instance.GetChunkPosAt(GlobalPosition);
		Chunk chunk = ChunkManager.Instance.GetChunkAt(GlobalPosition);
		Vector3I posInChunk = ChunkManager.Instance.GetPosInChunk(GlobalPosition);
		int height = chunk.GetHeightAt(new Vector2I(posInChunk.X, posInChunk.Z));
		GlobalPosition = GlobalPosition * new Vector3(1, 0, 1) + height * Vector3.Up;
	}

	private ushort GetCollidingBlock() => _chunkManager.GetChunkAt(GlobalPosition).GetBlock(_chunkManager.GetPosInChunk(GlobalPosition));

	public override void _PhysicsProcess(double delta)
	{
		ushort feetBlock = GetCollidingBlock();
		bool _wasInWater = _isInWater;
		_isInWater = feetBlock == (ushort)Blocks.DefaultBlock.Water;

		Vector3 velocity = Velocity;
		
		if (!IsOnFloor() && _gravityEnabled)
		{
			velocity += GetGravity() * Weight * (float)delta;
		}
		
		if (Input.IsActionPressed("jump") && (IsOnFloor() || _isInWater))
		{
			velocity.Y = _isInWater ? _waterJumpVelocity : JumpVelocity 
			            * (!_isInWater && _wasInWater ? 2 : 1); // Add boost when exiting water
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

		GD.Print(_isInWater);
		float dragForce = _isInWater ? _waterDrag : 0;
		Vector3 dragVector = -velocity * dragForce;
		velocity += dragVector * (float)delta;
		
		if (velocity.Length() < 0.1f)
			velocity = Vector3.Zero;

		Velocity = velocity;
		MoveAndSlide();
	}
}