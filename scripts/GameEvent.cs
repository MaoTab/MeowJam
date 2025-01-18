using System;
using Godot;
namespace Jam;

public struct GameEvent
{
    public static Action<Vector2> OnMouseLeftDown;
    public static Action<Vector2> OnMouseLeftDownInScene;
    public static Action OnMouseScrollWheelUp;
    
    
}