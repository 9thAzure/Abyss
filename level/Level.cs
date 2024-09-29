using Godot;
using System;

public partial class Level : Node2D
{
    PackedScene _finishScene = null!;
    Generator _generator = null!;
    Node2D _platforms = null!;
    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        _finishScene = GD.Load<PackedScene>("res://level/Finish/Finish.tscn");
        _generator = GetNode<Generator>("Generator");
        _platforms = GetNode<Node2D>("Platforms");
        Regenerate();
    }

    public void Regenerate()
    {
        foreach (var child in _platforms.GetChildren())
        {
            child.QueueFree();
        }

        _generator.GenerateLevel(5);
        Finish finish = _finishScene.Instantiate<Finish>();
        GD.Print(finish);
        _platforms.GetChild(-1).AddChild(finish);
        finish.Connect(Finish.SignalName.BodyEntered, Callable.From<Node2D>((_) => Regenerate()));
    }
}
