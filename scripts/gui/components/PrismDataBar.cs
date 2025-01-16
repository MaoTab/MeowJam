using System;
using System.Threading.Tasks;
using Godot;

namespace Jam;

public partial class PrismDataBar : Control
{
    [Export] public RichTextLabel TextLabel { get; set; }
    [Export] public SplitContainer Bar { get; set; }
    
    private const int TotalStages = 4; // 进度条分为4个阶段
    private const int TotalLength = 230; // 进度条的总长度

    public async Task ChangeBarAsync(float percentage)
    {
        // 计算每个阶段的目标长度
        float targetLength = TotalLength * percentage; // 计算目标长度

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

            // 更新 TextLabel 的文本，为当前阶段
            int currentStage = Mathf.Clamp((int)(currentLength / (TotalLength / TotalStages)), 0, TotalStages);
            TextLabel.Text = $"{currentStage}"; // 显示当前阶段
            
            // 等待一段时间以创建动画效果
            await Task.Delay(16); // 大约60 FPS，16毫秒每帧
        }

        // 确保最后的值是目标值，并更新文本
        Bar.SplitOffset = (int)targetLength; 
        int finalStage = Mathf.Clamp((int)(targetLength / (TotalLength / TotalStages)), 0, TotalStages);
        TextLabel.Text = $"{finalStage}"; // 最终更新文本
    }
}