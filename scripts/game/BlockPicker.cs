using System;
using System.Linq;
using Godot;
using Minecraft.scripts.engine;

namespace Minecraft.scripts.game;

public partial class BlockPicker : Node3D
{
    [Export] private MeshInstance3D _blockMesh;

    private int _selectedBlockIndex = 0;

    public int SelectedBlockIndex
    {
        get => _selectedBlockIndex;
        set
        {
            _selectedBlockIndex = (value % _blocks.Length + _blocks.Length) % _blocks.Length;
            UpdateSelectedBlock();
        }
    }

    private Blocks.DefaultBlock[] _blocks = Enum.GetValues<Blocks.DefaultBlock>().Where(b => b != Blocks.DefaultBlock.Air).ToArray();

    public override void _Ready()
    {
        UpdateSelectedBlock();
    }

    private void UpdateSelectedBlock()
    {
        _blockMesh.Mesh = PreviewBlockGenerator.GenerateBlockMesh(_blocks[_selectedBlockIndex]);
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event is InputEventMouseButton {Pressed: true} mouseButtonEvent)
        {
            if (mouseButtonEvent.ButtonIndex == MouseButton.WheelDown)
                SelectedBlockIndex++;
            else if (mouseButtonEvent.ButtonIndex == MouseButton.WheelUp)
                SelectedBlockIndex--;
        }
    }
}