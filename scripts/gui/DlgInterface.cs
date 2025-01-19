using System;
using System.Threading;
using System.Threading.Tasks;
using Godot;
using Godot.Collections;
using Jam.scripts;

namespace Jam;

public partial class DlgInterface : Control, IUi
{
    
    [Export] private AnimationPlayer animationPlayer;
    [Export] private ScrollContainer scrollContainer;
    [Export] public Control DlgTextList;
    [Export] private PackedScene _optionButtonPackedScene;
    [Export] private Control _optionBox;
    
    [Export] private PackedScene _separatorPackedScene;
    
    [Export] private PackedScene _dlgPartPackedScene;
    [Export] private TextureRect _headTextureRect;
    
    [Export] private CompressedTexture2D _bioHeadTexture;
    [Export] private CompressedTexture2D _psyHeadTexture;
    [Export] private CompressedTexture2D _socHeadTexture;
    
    [Export] private AnimationPlayerPlus _headAnimationPlayer;
    
    [Export] private AudioStreamPlayer _audioStreamPlayer;
    
    [Export] private AudioStreamWav bioWav;
    [Export] private AudioStreamWav psyWav;
    [Export] private AudioStreamWav socWav;
    
    /// <summary>
    /// 逐段打印时使用的结束符
    /// </summary>
    private string _typingText = "_";
    
    public EUIState State { get; set; }
    
    /// <summary>
    /// 锁定滚动
    /// </summary>
    private bool _lockScroll;
    
    public void Init()
    {
        _headAnimationPlayer.Play("RESET");
        GameEvent.OnMouseScrollWheelUp += () =>
        {
            _lockScroll = true;
        };
    }

    public void PhysicsProcess()
    {
        if (!_lockScroll)
        {
            scrollContainer.ScrollVertical += 10;    
        }
    }
    
    public void PlayAudio()
    {
        _audioStreamPlayer.Play();
    }
    
    /// <summary>
    /// 上一个对话的角色名
    /// </summary>
    private string preName = "";

    /// <summary>
    /// 如果为true则头像已隐藏
    /// </summary>
    private bool headHide = true;
    
    // MARK: - AddSeparator()
    public void AddSeparator(string text)
    {
        var node = _separatorPackedScene.Instantiate();
        if (node is not HDlgSeparator hdlgSeparator)
        {
            return;
        }
        
        _lockScroll = false;
        hdlgSeparator.label.Text = text;
        
        DlgTextList.AddChild(node);
    }
    
    // MARK: - AddDlgText()
    public async Task AddDlgText(string name, string newLine,Action onFinish)
    {
        var node = _dlgPartPackedScene.Instantiate();
        if (node is not DlgPart dlgPart)
        {
            return;
        }
        
        _lockScroll = false;
        DlgTextList.AddChild(node);
        
        string showName;
        
        if (string.IsNullOrEmpty(name))
        {
            showName = "";
        }
        else
        {
            // 更换文本音效
            switch (name)
            {
                case "肉体":
                    _audioStreamPlayer.Stream = bioWav;
                    showName = $"[color=#ff6188]{name}[/color]：";
                    break;
                case "理性":
                    _audioStreamPlayer.Stream = psyWav;
                    showName = $"[color=#6dc0ca]{name}[/color]：";
                    break;
                case "情感":
                    _audioStreamPlayer.Stream = socWav;
                    showName = $"[color=#b180d3]{name}[/color]：";
                    break;
                default:
                    showName = name + "：";
                    break;
            }
        }
        
        dlgPart.Creat(showName,true,newLine,onFinish);

        if (preName != name)
        {
            preName = name;

            if (!headHide)
            {
                headHide = true;
                await _headAnimationPlayer.PlayAsync("head/Hide");
            }
            
            // TODO:在这里做更换对应发言人的头像 
            switch (name)
            {
                case "肉体":
                    _headTextureRect.Texture = _bioHeadTexture;
                    _headTextureRect.Modulate = new Color(1, 0.38f, 0.53f);
                    break;
                case "理性":
                    _headTextureRect.Texture = _bioHeadTexture;
                    _headTextureRect.Modulate = new Color(0.43f, 0.75f, 0.79f);
                    break;
                case "情感":
                    _headTextureRect.Texture = _bioHeadTexture;
                    _headTextureRect.Modulate = new Color(0.65f, 0.60f, 0.93f);
                    break;
                default:
                    return;
            } 
            
            await _headAnimationPlayer.PlayAsync("head/Show");
            headHide = false;
        }
        
        // if (preName != name)
        // {
        //     preName = name;
        //     dlgPart.Creat(showName,true,newLine,onFinish);
        // }
        // else
        // {
        //     dlgPart.Creat(showName,false,newLine,onFinish);
        // }
    }
    
    /// <summary>
    /// 添加选项
    /// </summary>
    /// <param name="optionContent">选项的内容</param>
    /// <param name="onClick">当选项被按下时</param>
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
            _lockScroll = false;
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
    
    // MARK: - RemovePreDlg()
    /// <summary>
    /// 移除上一个对话框
    /// </summary>
    public void RemovePreDlg()
    {
        DlgTextList.GetChildren()[^1].QueueFree();
    }
    
    public Action OnAnimationFinish { get; set; }
    
    public new void Show()
    {
        animationPlayer.Play("dlg/Show");

        void OnFinish(StringName s)
        {
            OnAnimationFinish?.Invoke();
            animationPlayer.AnimationFinished -= OnFinish;
        }

        animationPlayer.AnimationFinished += OnFinish;
    }
    
    public new void Hide()
    {
        animationPlayer.PlayBackwards("dlg/Show");
    }
    
    
    // 下面是逼养的检定表现功能区
    [Export] private Array<AnimatedSprite2D> dicePoint;
    [Export] private Array<Control> dices;
    [Export] private RichTextLabel checkResText;
    [Export] private AnimationPlayerPlus checkResAnimationPlayer;
    
    private Random random = new();
    
    public async Task<bool> Check(int d,int dc)
    {
        // 骰子数必须在1到4之间
        if (d is < 1 or > 4)
        {
            return false;
        }
        
        foreach (var dice in dices)
        {
            dice.Visible = false;
        }
        
        int[] results = new int[d];

        // 生成每个骰子的结果
        for (int i = 0; i < d; i++)
        {
            var point = random.Next(1, 7); // 生成1到6之间的随机数
            dices[i].Visible = true;
            results[i] = point;
            dicePoint[i].Play(point.ToString());
        }   
        var allResults = 0;
        
        foreach (var result in results)
        {
            allResults += result;
        }

        checkResText.Text = allResults > dc ? "[color=#9cca6f]成功[/color]" : "[color=#ff6188]失败[/color]";
        
        await checkResAnimationPlayer.PlayAsync("check/Show");
        
        return allResults > dc;
    }
}