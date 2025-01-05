using System;
using System.Threading;
using System.Threading.Tasks;
using Godot;

namespace Jam;

public partial class DlgInterface : Control, IUi
{

    [Export] private PackedScene _dlgTextPackedScene;
    [Export] public Control DlgTextList;
    [Export] private PackedScene _optionButtonPackedScene;
    [Export] private Control _optionBox;
    
    /// <summary>
    /// 逐段打印时使用的结束符
    /// </summary>
    private string _typingText = "_";
    
    public EUIState State { get; set; }
    public void Init()
    {
       
    }
    
    // MARK: - AddDlgText()
    public async Task AddDlgText(string newLine)
    {
        await UpdateText("System", newLine);
    }
    
    public async Task UpdateText(string name, string newLine)
    {
        string fullText = newLine; // 使用 newLine 作为全文本

        var node = _dlgTextPackedScene.Instantiate();
        if (node is not RichTextLabel textLabel)
        {
            return;
        }

        DlgTextList.AddChild(node);
        
        textLabel.Text = ""; // 清空现有文本
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
                            await Task.Delay((int)(0.05f * 1000 * seconds)); // 等待指定秒数
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
                        textLabel.Text = currentText;
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
                    textLabel.Text = currentText;
                    i = endIndex + 1;
                    insideTag = false; // 重置为不在标签内部
                    continue; // 跳过下一个字符的处理
                }
            }

            // 添加当前字符
            currentText += fullText[i];
            textLabel.Text = currentText;

            // 仅在不在 BBCode 标签内部时添加下划线
            if (!insideTag && i < fullText.Length - 1)
            {
                textLabel.Text = currentText + _typingText;
            }

            await Task.Delay((int)(0.05f * 1000)); // 转换为毫秒
            i++;

            // 检查是否进入 BBCode 标签内部
            if (i < fullText.Length && fullText[i] == '[')
            {
                insideTag = true;
            }
        }

        // 完全输出后，确保最后没有下划线
        textLabel.Text = textLabel.Text.TrimEnd(_typingText.ToCharArray()); // 确保最后没有下划线
    }
    
    /// <summary>
    /// 添加选项
    /// </summary>
    /// <param name="optionContent">选项的内容</param>
    /// <param name="onClick">当选项被按下时</param>
    /// <param name="record">选择选项后向对话历史输入的内容</param>
    /// <param name="onEnter">鼠标悬停在选项上时</param>
    /// <param name="onExit">鼠标离开选项上时</param>
    /// <param name="onHoldDown">鼠标按住事件</param>
    /// <param name="waitTime">创建延迟，用于逐个弹出效果</param>
    // MARK: - AddOption()
    public async Task AddOption(string optionContent, Action onClick, Action onEnter = null,
        Action onExit = null, Action onHoldDown = null, int waitTime = 0)
    {
        var node = _optionButtonPackedScene.Instantiate();
        
        _optionBox.AddChild(node);

        if (node is MButton button)
        {
            button.TextLabel.Text = optionContent;

            // 实现选项逐个弹出的效果
            if (waitTime != 0)
                await Task.Delay(waitTime);

            if (onEnter != null)
            {
                button.ButtonBase.MouseEntered += onEnter.Invoke;
            }

            if (onExit != null)
            {
                button.ButtonBase.MouseExited += onExit.Invoke;
            }

            bool isHolding = false;
            bool holdTriggered = false; // 新增变量，用于标识是否触发了长按
            CancellationTokenSource holdCancellationTokenSource = new CancellationTokenSource();

            if (onHoldDown != null)
            {
                async void OnButtonBaseOnButtonDown()
                {
                    holdCancellationTokenSource = new CancellationTokenSource();
                    isHolding = true;
                    holdTriggered = false; // 重置标识
                    try
                    {
                        // 等待一定时间后调用 onHoldDown
                        await Task.Delay(1000, holdCancellationTokenSource.Token); // 1秒钟的按住时间
                        if (isHolding)
                        {
                            var (x, y) = Game.MousePos;
                            var (x1, y1) = button.Size;
                            var (f, f1) = button.GlobalPosition;
                            // 检查鼠标是否在按钮内
                            if (!(x >= f) || !(x <= f + x1) || !(y >= f1) || !(y <= f1 + y1)) return;

                            onHoldDown.Invoke();
                            holdTriggered = true; // 标记为已触发长按
                        }
                    }
                    catch (TaskCanceledException)
                    {
                        // 被取消时后注销Token，实际上好像并没有什么卵用，已经被删了
                        holdCancellationTokenSource.Dispose();
                    }
                }

                button.ButtonBase.ButtonDown += OnButtonBaseOnButtonDown;
            }

            button.ButtonBase.ButtonUp += () =>
            {
                if (isHolding)
                {
                    // 重置持有状态
                    isHolding = false;
                    holdCancellationTokenSource.Cancel(); // 取消持有的任务
                }

                if (holdTriggered)
                {
                    // 如果已经触发了长按，不执行 onClick
                    holdTriggered = false; // 重置标识
                    return;
                }

                var (x, y) = Game.MousePos;
                var (x1, y1) = button.Size;
                var (f, f1) = button.GlobalPosition;

                // 检查鼠标是否在按钮内，用于满足有人不小心按下去但是还没松开准备反悔的情况
                if (!(x >= f) || !(x <= f + x1) || !(y >= f1) || !(y <= f1 + y1)) return;

                onClick.Invoke();
            };
        }
    }
    
    // MARK: - RemoveAllOption()
    public void RemoveAllOption()
    {
        foreach (var node in _optionBox.GetChildren())
        {
            node.QueueFree();
        }
    }
    
    // MARK: - RemoveAllDlg()
    public void RemoveAllDlg()
    {
        foreach (var node in DlgTextList.GetChildren())
        {
            node.QueueFree();
        }
    }
}