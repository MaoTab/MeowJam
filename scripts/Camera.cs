using Godot;

namespace Jam;

public partial class Camera : Node2D
{
    public void Init()
    {
        
    }

    public void PhysicsProcess()
    {
        Position = new Vector2(
            Mathf.Lerp(Position.X + 10, Game.ControlRole.Position.X, (float)(3 * Game.PhysicsDelta)),
            Mathf.Lerp(Position.Y, Game.ControlRole.Position.Y, (float)(3 * Game.PhysicsDelta)));
    }
}