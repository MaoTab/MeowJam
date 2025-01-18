using System.Threading;
using System.Threading.Tasks;
using Godot;

namespace Jam;

public partial class VoteBar  : Control
{
    [Export] public RichTextLabel TextLabel { get; set; }
    [Export] public SplitContainer Bar { get; set; }
    
    private const int TotalLength = 550; // 进度条的总长度
    
    private CancellationTokenSource _cancellationTokenSource; // 取消令牌源

    public async Task ChangeBarAsync(float percentage,int totalStages)
    {
        // 如果已有动画在进行，取消它
        _cancellationTokenSource?.Cancel();
        _cancellationTokenSource = new CancellationTokenSource();
        CancellationToken token = _cancellationTokenSource.Token;

        // 计算每个阶段的目标长度
        float targetLength = TotalLength * (1 - percentage); // 因为进度条竖起来了，故反转长度，0为满，1为空

        // 获取当前长度
        float currentLength = Bar.SplitOffset; 

        float duration = 1.0f; // 动画持续时间
        float elapsedTime = 0.0f; // 已经过的时间

        // 动画循环
        while (elapsedTime < duration)
        {
            elapsedTime += (float)Game.PhysicsDelta; // 获取每帧的时间
            float t = Mathf.Clamp(elapsedTime / duration, 0.0f, 1.0f); // 计算插值因子，范围在 [0, 1] 之间

            // 使用插值
            currentLength = Mathf.Lerp(currentLength, targetLength, t);
            Bar.SplitOffset = (int)currentLength;

            // 更新文本，为当前阶段
            int currentStage = Mathf.Clamp((int)(currentLength / (TotalLength / totalStages)), 0, totalStages);
            TextLabel.Text = $"{totalStages - currentStage}"; // 显示当前阶段
            
            // 检查是否已请求取消
            if (token.IsCancellationRequested)
            {
                break;
            }
            
            await Task.Delay(16);
        }

        // 如果不是被取消的，确保最后的值是目标值，并更新文本
        if (!token.IsCancellationRequested)
        {
            // 确保最后的值是目标值，并更新文本
            Bar.SplitOffset = (int)targetLength; 
            int finalStage = Mathf.Clamp((int)(targetLength / (TotalLength / totalStages)), 0, totalStages);
            TextLabel.Text = $"{totalStages - finalStage}";
        }
    }
}