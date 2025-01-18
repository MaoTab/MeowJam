using Newtonsoft.Json;

namespace Jam.data;

public struct PrismData
{
    public void Init(PrismData prismData)
    {
        MaxLevel = prismData.MaxLevel;
        MinLevel = prismData.MinLevel;
        Level = prismData.Level;
    }

    private int level;

    /// <summary>
    /// 当前等级
    /// </summary>
    public int Level
    {
        get => level;
        set { level = int.Clamp(value, MinLevel, MaxLevel); }
    }

    /// <summary>
    /// 最大等级
    /// </summary>
    public int MaxLevel { get; set; }

    /// <summary>
    /// 最小等级
    /// </summary>
    public int MinLevel { get; set; }
}