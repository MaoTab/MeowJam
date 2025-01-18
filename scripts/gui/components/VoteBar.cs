using System.Threading;
using System.Threading.Tasks;
using Godot;

namespace Jam;

public partial class VoteBar  : TextureProgressBar
{
    private const int PointTotalLength = 520; // 进度条的总长度

    [Export] private SplitContainer Point;
    
    private CancellationTokenSource _cancellationTokenSource; // 取消令牌源

    public async Task ChangeBarAsync(float percentage,int totalStages)
    {
        // 如果已有动画在进行，取消它
        _cancellationTokenSource?.Cancel();
        _cancellationTokenSource = new CancellationTokenSource();
        CancellationToken token = _cancellationTokenSource.Token;

        // 计算每个阶段的目标长度
        float targetLength = (float)MaxValue * percentage; // 因为进度条竖起来了，故反转长度，0为满，1为空
        float pointTargetLength = PointTotalLength * percentage; // 因为进度条竖起来了，故反转长度，0为满，1为空

        float currentLength = Point.SplitOffset; 
        
        float duration = 1.0f; // 动画持续时间
        float elapsedTime = 0.0f; // 已经过的时间

        // 动画循环
        while (elapsedTime < duration)
        {
            elapsedTime += (float)Game.PhysicsDelta; // 获取每帧的时间
            float t = Mathf.Clamp(elapsedTime / duration, 0.0f, 1.0f); // 计算插值因子，范围在 [0, 1] 之间
            
            // 使用插值
            currentLength = Mathf.Lerp(currentLength, pointTargetLength, t);
            Point.SplitOffset = (int)currentLength;
            Value = Mathf.Lerp(Value, targetLength, t);
            
            // 检查是否已请求取消
            if (token.IsCancellationRequested)
            {
                break;
            }
            
            await Task.Delay(16);
        }
    }
}