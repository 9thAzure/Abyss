using Godot;
using System;

public partial class Finish : Area2D
{
    Node2D _shape = null!;
    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        _shape = GetNode<Node2D>("Polygon2D");
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
        _shape.Rotation += 6 * (float)delta;
    }
}
