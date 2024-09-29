using Godot;
using System;

public partial class Player : CharacterBody2D
{
    [Export]
    public double MaxSpeed { get; set; } = 10;
    [Export]
    public double Acceleration { get; set; } = 5;


    [Export]
    public int HeightCurve
    {
        get => _heightCurve;
        set
        {
            double oldHeight = MaxHeight;// _gravityMultiplier /= _heightCurve * value;
            _heightCurve = value;
            MaxHeight = oldHeight;
        }
    }
    private int _heightCurve = 3;
    [Export]
    public double MaxHeightTime { get; set; } = 1;
    [Export]
    public double MaxHeight
    {
        get => Math.Pow(MaxHeightTime, 2 * HeightCurve) * _gravityMultiplier / (2 * HeightCurve);
        set
        {
            _gravityMultiplier = 2 * HeightCurve * value / Math.Pow(MaxHeightTime, 2 * HeightCurve);
        }
    }
    private double _gravityMultiplier = 10;

    private double _timeSinceLastJump = 0.0;
    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        _timeSinceLastJump = MaxHeightTime * 2;
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
        var velocity = Velocity;
        _timeSinceLastJump += delta;

        velocity.X *= 0.98f;
        if (IsOnFloor())
        {
            velocity.X *= 0.98f;

            velocity.Y = 0.0f;
            _timeSinceLastJump = MaxHeightTime;
            if (Input.IsActionPressed("ui_up"))
            {
                // GD.Print("jump!");
                _timeSinceLastJump = 0;
            }
        }
        velocity.Y = (float)HeightChange(_timeSinceLastJump);

        if (Input.IsActionPressed("ui_right"))
        {
            velocity.X = (float)Math.Min(velocity.X + Acceleration * delta, MaxSpeed);
        }
        if (Input.IsActionPressed("ui_left"))
        {
            velocity.X = (float)Math.Max(velocity.X - Acceleration * delta, -MaxSpeed);
        }

        Velocity = velocity;
        MoveAndSlide();
    }

    public double HeightChange(double t) => _gravityMultiplier * Math.Pow(t - MaxHeightTime, 2 * HeightCurve - 1);
    public double JumpTrajectory(double t) => MaxHeight / 2 / HeightCurve * (Math.Pow(t - MaxHeightTime, 2 * HeightCurve) - Math.Pow(MaxHeightTime, 2 * HeightCurve));
}
