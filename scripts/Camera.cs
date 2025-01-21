using Godot;

namespace SnowBlindness;

public partial class Camera : Node2D
{
    public void Init()
    {
        
    }

    [Export] public float MinZoomSize = 1.29f;
    [Export] public float MaxZoomSize = 2.42f;
    
    public void PhysicsProcess()
    {
        Position = new Vector2(
            Mathf.Lerp(Position.X + 10, Game.ControlRole.Position.X, (float)(3 * Game.PhysicsDelta)),
            Mathf.Lerp(Position.Y, Game.ControlRole.Position.Y, (float)(3 * Game.PhysicsDelta)));
    }
}