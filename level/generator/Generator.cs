using System;
using Godot;

public partial class Generator : Node
{
    [Export]
    public Player Player { get; set; } = null!;
    [Export]
    public double PlayerHeight { get; set; } = 12;
    [Export]
    public Node2D Level { get; set; } = null!;
    [ExportCategory("config")]
    [Export]
    public int GenerationAttempts { get; set; } = 8;
    [Export]
    public double Margin { get; set; } = 0.95;
    [Export]
    public int PathResolution { get; set; } = 10;


    public void GenerateLevel(int length)
    {
        Debug.Assert(Player is not null);
        Debug.Assert(Level is not null);

        var platformScene = GD.Load<PackedScene>("res://level/platforms/basic_platform.tscn");
        List<Platform> mainPlatforms = new();
        List<List<Vector2>> paths = new();

        Vector2 maxJumpDistance = new Vector2((float)Player.MaxHeightTime * (float)Player.MaxSpeed, -(float)Player.MaxHeight) * (float)Margin;
        Vector2 maxFlatDistance = Vector2.Right * (float)Player.MaxHeightTime * 2 * (float)Player.MaxSpeed * (float)Margin;

        Vector2[] possiblePositions = { Vector2.Zero, Vector2.Up * -maxJumpDistance.Y, maxJumpDistance, maxFlatDistance };

        var currentPlatform = platformScene.Instantiate<Platform>();
        currentPlatform.Position = new Vector2((float)Random.Shared.NextDouble() * 900 + 100, 612);
        mainPlatforms.Add(currentPlatform);
        // paths.Add(new(2) { currentPlatform.LeftPoint + currentPlatform.Position + Vector2.Up * (float)PlayerHeight, currentPlatform.RightPoint + currentPlatform.Position + Vector2.Up * (float)PlayerHeight });
        Player.Position = currentPlatform.Position + (currentPlatform.LeftPoint + currentPlatform.RightPoint) / 2 + Vector2.Up * 10;

        while (length > 0)
        {
            GD.Print($"{length} platforms remaining");
            var previousPlatform = mainPlatforms[^1];
            currentPlatform = platformScene.Instantiate<Platform>();
            ConcavePolygonShape2D path = new();

            // decide on next platform placement.
            int attempts;
            for (attempts = 0; attempts < GenerationAttempts; attempts++)
            {
                GD.Print($"attempt {attempts}");
                int multiplier = Random.Shared.Next(2) * 2 - 1;
                bool startFromRight = multiplier == 1;
                Vector2 startEdge = previousPlatform.Position + (startFromRight ? previousPlatform.RightPoint : previousPlatform.LeftPoint);

                var platformEdge = RandomPossiblePosition();
                platformEdge.X *= multiplier;
                currentPlatform.Position = platformEdge - (startFromRight ? currentPlatform.LeftPoint : currentPlatform.RightPoint) + startEdge;

                if (IsPlatformIntersectingPath(currentPlatform))
                {
                    continue;
                }

                // Generate Jump Path.
                List<Vector2> points = new(PathResolution * 2)
                {
                    startEdge + Vector2.Up * (float)PlayerHeight,
                    (startFromRight ? currentPlatform.LeftPoint : currentPlatform.RightPoint) + currentPlatform.Position + Vector2.Up * (float)PlayerHeight
                };
                // Level.AddChild(new Drawer()
                // {
                //     Points = points
                // });

                // double flightTime = Player.MaxHeightTime + Math.Pow(Math.Pow(Player.MaxHeightTime, 2 * Player.HeightCurve) + 2 * Player.HeightCurve * platformEdge.Y / Player.MaxHeight, 1 / 2.0 / Player.MaxHeightTime);
                // double trajectoryEnd = flightTime * Player.MaxSpeed;
                // double speedFactor = platformEdge.X / trajectoryEnd;

                // for (int i = 0; i < PathResolution; i++)
                // {
                //     double time = flightTime / PathResolution * (i + 1);
                //     points.Add(new Vector2((float)(time * Player.MaxSpeed * speedFactor), (float)Player.JumpTrajectory(time)) * multiplier + startEdge + Vector2.Up * (float)PlayerHeight);
                //     // if (i + 1 != PathResolution)
                //     // {
                //     //     points.Add(points[^1]);
                //     // }
                // }

                // path.Segments = points.ToArray();

                mainPlatforms.Add(currentPlatform);
                paths.Add(points);
                break;
            }
            if (attempts == GenerationAttempts)
            {
                GD.Print("recursive retry");
                mainPlatforms.RemoveAt(mainPlatforms.Count - 1);
                paths.RemoveAt(paths.Count - 1);
                length++;
                continue;
            }

            length--;
        }

        foreach (var platform in mainPlatforms)
        {
            Level.AddChild(platform);
        }

        Vector2 RandomPossiblePosition()
        {
            int length = possiblePositions.Length;
            var point1 = possiblePositions[Random.Shared.Next(length)];
            var point2 = possiblePositions[Random.Shared.Next(length)];
            var point3 = possiblePositions[Random.Shared.Next(length)];
            var point4 = possiblePositions[Random.Shared.Next(length)];
            return point1.Lerp(point2, Random.Shared.NextSingle()).Lerp(point3.Lerp(point4, Random.Shared.NextSingle()), Random.Shared.NextSingle());
        }

        bool IsPlatformIntersectingPath(Platform platform) 
        {
            // var shape = platform.GetNode<CollisionShape2D>("CollisionShape2D").Shape;
            // var transform = platform.Transform;
            Vector2 start = platform.LeftPoint + platform.Position - Vector2.Down * 4;
            Vector2 end = platform.RightPoint + platform.Position - Vector2.Down * 4;
            // Level.AddChild(new Drawer()
            // {
            //     Points = new() {start, end},
            //     Modulate = new Color(1, 1, 0.5f, 0.5f)
            // });
            foreach (var path in paths)
            {
                // GD.Print(path);
                int size = path.Count - 1;
                for (int i = 0; i < size; i++)
                {
                    // GD.Print(path[i]);
                    var result = Geometry2D.SegmentIntersectsSegment(start, end, path[i], path[i + 1]);
                    if (result.VariantType is not Variant.Type.Nil)
                    {
                        GD.Print(result.Obj);
                        return true;
                    }
                }
            }
            foreach (var otherPlatform in mainPlatforms)
            {
                if (IsInvalid(otherPlatform.LeftPoint + otherPlatform.Position) ||
                    IsInvalid(otherPlatform.RightPoint + otherPlatform.Position))
                {
                    return true;
                }

                bool IsInvalid(Vector2 point)
                {
                    if (point.X < start.X || point.X > end.X)
                    {
                        return false;
                    }
                    if (Math.Abs(point.Y - start.Y) < PlayerHeight * 2)
                    {
                        return true;
                    }
                    return false;
                }
            }
            return false;
        }
        // bool IsPathIntersectingPlatform
    }

    // private Vector2

    // public partial class Drawer : Node2D
    // {
    //     public List<Vector2> Points { get; set; } = null!;
    //     public override void _Draw()
    //     {
    //         QueueFree();
    //         DrawPolyline(Points.ToArray(), Colors.White, 5);
    //     }
    // }
}