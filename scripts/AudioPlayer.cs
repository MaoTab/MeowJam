using System.Threading.Tasks;
using Godot;

namespace Jam.scripts;

public partial class AudioPlayer : AudioStreamPlayer
{
    public async Task PlayAsync()
    {
        Play();
        
        // 持续循环，直到动画播放完毕
        while (Playing)
        {
            // 等待下一帧
            await Task.Delay(10); // 转换为毫秒
        }
    }
}