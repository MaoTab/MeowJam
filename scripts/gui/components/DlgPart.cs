using System;
using System.Threading.Tasks;
using Godot;

namespace SnowBlindness;

public partial class DlgPart : Control
{
    /// <summary>
    /// 名称的框
    /// </summary>
    [Export]
    private Control NameBox { get; set; }
    
    /// <summary>
    /// 名称的文本
    /// </summary>
    [Export]
    private RichTextLabel NameTextLabel { get; set; }
    
    /// <summary>
    /// 逐段打印时使用的结束符
    /// </summary>
    private string _typingText = "_";
    
    /// <summary>
    /// 水平框，用于规范文本靠左还是靠右
    /// </summary>
    [Export]
    private Control HBox { get; set; }
    
    /// <summary>
    /// 占位配置边距的文本
    /// </summary>
    [Export]
    private RichTextLabel SizeTextLabel { get; set; }

    /// <summary>
    /// 播放打字动画的文本
    /// </summary>
    [Export]
    private RichTextLabel AnimTextLabel { get; set; }

    [Export] private AnimationPlayer AnimationPlayer { get; set; }
    
    /// <summary>
    /// 文本播放速度
    /// </summary>
    [Export] private float TextTypingSpeed = 0.01f;
    public void Creat(string name,bool showName, string newLine, Action onFinish)
    {
        SizeTextLabel.Text = showName + newLine.Replace("[br/]", "\n");

        if (!showName)
        {
            NameBox.Visible = false;
        }
        else
        {
            NameTextLabel.Text = name;
        }
        
        string fullText = name + newLine; // 使用 newLine 作为全文本
        
#pragma warning disable VSTHRD101
        
        AnimationPlayer.AnimationFinished += async _ =>
        {
            int i = 0;
            bool insideTag = false; // 用于跟踪是否在 BBCode 标签内部
            string currentText = ""; // 存储当前文本内容

            // 打印机主体
            while (i < fullText.Length)
            {
                if (fullText[i] == '[') // 检测到 BBCode 开始标签
                {
                    int endIndex = fullText.IndexOf(']', i);
                    if (endIndex != -1)
                    {
                        string tag = fullText.Substring(i, endIndex - i + 1);

                        // 检查是否为 [stop=数字] 标签
                        if (tag.StartsWith("[stop=") && tag.EndsWith("/]"))
                        {
                            // 提取数字
                            if (int.TryParse(tag.Substring(6, tag.Length - 8), out int seconds))
                            {
                                await Task.Delay((int)(TextTypingSpeed * 1000 * seconds)); // 等待指定秒数
                            }
                        }
                        else if (tag.StartsWith("[br") && tag.EndsWith("]")) // 换行
                        {
                            tag = "\n";
                            currentText += tag; // 其他 BBCode 标签直接添加
                        }
                        else
                        {
                            currentText += tag; // 其他 BBCode 标签直接添加
                            AnimTextLabel.Text = currentText;
                        }

                        i = endIndex + 1;
                        insideTag = false; // 重置为不在标签内部
                        continue; // 跳过下一个字符的处理
                    }
                }
                else if (fullText[i] == '/' && i > 0 && fullText[i - 1] == '[') // 检测到 BBCode 结束标签
                {
                    int endIndex = fullText.IndexOf(']', i);
                    if (endIndex != -1)
                    {
                        string tag = fullText.Substring(i - 1, endIndex - i + 2);
                        currentText += tag;
                        AnimTextLabel.Text = currentText;
                        i = endIndex + 1;
                        insideTag = false; // 重置为不在标签内部
                        continue; // 跳过下一个字符的处理
                    }
                }

                // 添加当前字符
                currentText += fullText[i];
                AnimTextLabel.Text = currentText;

                // 仅在不在 BBCode 标签内部时添加下划线
                if (!insideTag && i < fullText.Length - 1)
                {
                    // 每X字播放一次
                    if (i % 4 == 0)
                    {
                        Game.Gui.DlgInterface.PlayAudio();
                    }
                    
                    AnimTextLabel.Text = currentText + _typingText;
                }

                await Task.Delay((int)(TextTypingSpeed * 1000)); // 转换为毫秒
                i++;

                // 检查是否进入 BBCode 标签内部
                if (i < fullText.Length && fullText[i] == '[')
                {
                    insideTag = true;
                }
            }

            // 完全输出后，确保最后没有下划线
            AnimTextLabel.Text = AnimTextLabel.Text.TrimEnd(_typingText.ToCharArray()); // 确保最后没有下划线
            
            onFinish?.Invoke();
        };
#pragma warning restore VSTHRD101
        
        if (name != null && name == "Player")
        {
            HBox.LayoutDirection = LayoutDirectionEnum.Rtl;
            
            AnimationPlayer.Play("dlg_part/Show_R");
        }
        else
        {
            AnimationPlayer.Play("dlg_part/Show");
        }
        
    }
}