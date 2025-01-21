using System;
using System.Collections.Generic;
using Godot;

namespace SnowBlindness;
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
    
    public static double Calculate4D6Rate(int d, int dc)
    {
        // 计算成功和失败的概率
        double successProbability = (7 - dc) / 6.0; // 每个骰子成功的概率
        double failureProbability = 1 - successProbability; // 每个骰子失败的概率

        // 计算所有骰子都失败的概率
        double allFailProbability = Math.Pow(failureProbability, d);

        // 计算至少一个骰子成功的概率
        double successRate = 1 - allFailProbability;

        return successRate;
    }
}