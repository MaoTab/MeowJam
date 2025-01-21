using System;
using Godot;

namespace SnowBlindness;

public static class Game
{
    /// <summary>
    /// 根节点
    /// </summary>
    public static Root Root { get; set; }
    
    public static scripts.Interface Gui;
    public static AudioMgr Audio;
    
    /// <summary>
    /// Yarn运行时
    /// </summary>
    public static YarnRuntime Yarn { get; set; }
    
    /// <summary>
    /// 鼠标在场景中的位置
    /// </summary>
    public static Vector2 SceneMousePos { get; set; }
    
    /// <summary>
    /// 鼠标的位置（基于屏幕）
    /// </summary>
    public static Vector2 MousePos { get; set; }
    
    /// <summary>
    /// 游戏关卡/场景
    /// </summary>
    public static Level Level { get; set; }
    
    /// <summary>
    /// 玩家操作的角色
    /// </summary>
    public static Role ControlRole{get;set;}
    
    /// <summary>
    /// 玩家角色数据
    /// </summary>
    public static PlayerData PlayerData { get; set; }
    
    /// <summary>
    /// 是否可操作
    /// </summary>
    public static bool CanControl{get;set;}

    
    /// <summary>
    /// 游戏物理时间差量
    /// </summary>
    public static Double PhysicsDelta{get;set;}
    
    /// <summary>
    /// 摄像机
    /// </summary>
    public static Camera Camera{get;set;}
    
    /// <summary>
    /// 当前选中对象
    /// </summary>
    public static ISelect Select;
}