
using System.Threading.Tasks;
using Godot;

namespace SnowBlindness.scripts;

public partial class AnimationPlayerPlus : AnimationPlayer
{
    
    public async Task PlayAsync(string animationName)
    {
        Play(animationName);
        
        // 持续循环，直到动画播放完毕
        while (IsPlaying() && GetCurrentAnimation() == animationName)
        {
            // 等待下一帧
            await Task.Delay(10); // 转换为毫秒
        }
        
        GD.Print($"动画 '{animationName}' 已完成。");
    }
}