using Godot;
using Godot.Collections;

namespace Jam;

public partial class Level : Node2D
{
    /// <summary>
    /// 预制点位(拥于预制的记录位置)
    /// </summary>
    [Export] public Dictionary<string,Node2D> Point;
    
    [Export] public Role PlayerNode;
    
    [Export] public Node2D AiNode;
    public Dictionary<string,Role> Ais = new();
    
    public Level Init()
    {
        Game.ControlRole = PlayerNode.Init();
        
        foreach (var node in AiNode.GetChildren())
        {
            if (node is Role role)
            {
                Ais.Add(role.Name,role);
            }
        }
        
        // foreach (var ai in Ais.Values)
        // {
        //     var aiId = ai.ID.ToString();
        //
        //     if (Game.Stream.CheckAIData(aiId))
        //     {
        //         ai.Init(Game.Stream.GetAIPos(aiId),Game.Stream.GetAIFace(aiId));
        //     }
        //     else
        //     {
        //         ai.Init();
        //     }
        // }
        
        return this;
    }

    public void PhysicsProcess()
    {
        Game.ControlRole.PhysicsProcess();
    }
}