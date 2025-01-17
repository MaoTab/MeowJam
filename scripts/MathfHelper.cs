using System;
using Godot;

namespace Jam;
public static class MathfHelper
{
    public static Vector2 V2Lerp(Vector2 a, Vector2 b, float t)
    {
        // 限制 t 在 0 到 1 之间
        t = Math.Clamp(t, 0f, 1f);
        return new Vector2(
            a.X + (b.X - a.X) * t,
            a.Y + (b.Y - a.Y) * t
        );
    }
    
    /// <summary>
    /// 求近值
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <param name="tolerance">差量</param>
    /// <returns></returns>
    public static bool AetF(float a, float b, float tolerance = 0.1f)
    {
        return Math.Abs(a - b) < tolerance;
    }
}