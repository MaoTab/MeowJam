using System.Threading;
using System.Threading.Tasks;
using Godot;

namespace Jam;

public partial class VoteBar  : Control
{
    [Export] public RichTextLabel TextLabel { get; set; }
    [Export] public SplitContainer Bar { get; set; }
    
    private const int TotalLength = 800; // 进度条的总长度
    
    
    private CancellationTokenSource _cancellationTokenSource; // 取消令牌源

    public async Task ChangeBarAsync(float percentage)
    {
        // 如果已有动画在进行，取消它
        _cancellationTokenSource?.Cancel();
        _cancellationTokenSource = new CancellationTokenSource();
        CancellationToken token = _cancellationTokenSource.Token;

        // 计算每个阶段的目标长度
        float targetLength = TotalLength * (1 - percentage); // 反转目标长度，0为满，1为空

        // 获取当前长度
        float currentLength = Bar.SplitOffset; 

        float duration = 1.0f; // 动画持续时间（秒）
        float elapsedTime = 0.0f; // 已经过的时间

        // 动画循环
        while (elapsedTime < duration)
        {
            elapsedTime += (float)Game.PhysicsDelta; // 获取每帧的时间
            float t = Mathf.Clamp(elapsedTime / duration, 0.0f, 1.0f); // 计算插值因子，范围在 [0, 1] 之间

            // 使用 Lerp 插值
            currentLength = Mathf.Lerp(currentLength, targetLength, t);
            Bar.SplitOffset = (int)currentLength;

            // 更新 TextLabel 的文本
            float currentPercentage = 1 - (currentLength / TotalLength); // 当前进度反转
            TextLabel.Text = $"{(currentPercentage * 100):0.00}%"; // 格式化为百分比

            // 检查是否已请求取消
            if (token.IsCancellationRequested)
            {
                // 选择中断动画时的处理逻辑
                break;
            }
            
            // 等待一段时间以创建动画效果
            await Task.Delay(16); // 大约60 FPS，16毫秒每帧
        }

        // 如果不是被取消的，确保最后的值是目标值，并更新文本
        if (!token.IsCancellationRequested)
        {
            Bar.SplitOffset = (int)targetLength; 
            TextLabel.Text = $"{((1 - percentage) * 100):0.00}%"; // 最终更新文本，反转百分比
        }
    }
}