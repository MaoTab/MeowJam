using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;

namespace Jam;

public partial class Interface : Control
{
    [Export] public DlgInterface DlgInterface;

    
     #region Tip部分
    
     private List<TipBox> _tipBoxes = new();
     // TIP的范围
     [Export] private Control _tipZone;
     [Export] private PackedScene _tipBoxPackedScene;
     private bool _asyncTip;
     private bool _lockTipPos;
     [Export] private Control _darkMask;
     
    // MARK: - GetLastTip()
    private bool TryGetLastTip(out TipBox lastTip)
    {
        if (_tipBoxes.Count > 0)
        {
            lastTip = _tipBoxes[^1];
            return true;
        }

        lastTip = null;
        return false;
    }

    public void RefreshListTip(List<string> metas)
    {
        if (TryGetLastTip(out var tipBox))
        {
            for (var index = 0; index < metas.Count; index++)
            {
                var meta = metas[index];

                if (meta == "line")
                {
                    continue;
                }

                if (tipBox.VBox.GetChild(index) is RichTextLabel textLabel)
                {
                    textLabel.Text = meta;
                }
            }
        }
    }

    [Export] private PackedScene _tipTextPackedScene;

    [Export] private PackedScene _tipHSeparatorPackedScene;
    
    // MARK: - CreatListTip()
    public void CreatListTip(List<string> metas, bool lockPos = false, string key = "")
    {
        var node = _tipBoxPackedScene.Instantiate();
        AddChild(node);
        if (node is not TipBox tipBox) return;
        _tipBoxes.Add(tipBox);
        
        tipBox.Key = key;
        foreach (var meta in metas)
        {
            if (meta == "line")
            {
                node = _tipHSeparatorPackedScene.Instantiate();
                if (node is not HSeparator line) continue;
                tipBox.VBox.AddChild(line);
                continue;
            }

            node = _tipTextPackedScene.Instantiate();
            if (node is not TipBuilder textLabel) continue;
            textLabel.LockPos = lockPos;
            tipBox.VBox.AddChild(textLabel);
            textLabel.Text = meta;
        }
        
        tipBox.Creat();
        _lockTipPos = lockPos;
        _asyncTip = true;
    }
    
    // MARK: - CreatTip()
    public void CreatTip(Variant meta, bool lockPos = false)
    {
        var node = _tipBoxPackedScene.Instantiate();
        AddChild(node);

        if (node is not TipBox tipBox) return;
        _tipBoxes.Add(tipBox);
        
        node = _tipTextPackedScene.Instantiate();
        if (node is not TipBuilder textLabel) return;
        textLabel.LockPos = lockPos;
        tipBox.VBox.AddChild(textLabel);

        
        textLabel.Text = Tr(meta.AsString());
        
        tipBox.Creat();
        _lockTipPos = lockPos;
        _asyncTip = true;
    }

    // MARK: - FreezeTipFromButton()
    public void FreezeTipFromButton()
    {
        if (!TryGetLastTip(out TipBox tip)) return;

        // 多个Tip出现时，再次叠加开始逐个将前一个Tip变暗
        if (_tipBoxes.Count - 1 > 0)
        {
            _tipBoxes[^2].Modulate = new Color(1, 1, 1, .5f);
            // 禁用上一段文本的Tip功能
            _tipBoxes[^2].MouseFilter = MouseFilterEnum.Ignore;
        }

        tip.Freeze = true;
        _darkMask.Visible = true;

        // 当鼠标点击任意空白区域
        tip.BindMouseExit((() =>
        {
            if (!TryGetLastTip(out TipBox preTip)) return;
            if (tip == preTip) UnFreezeTip();
        }));
    }

    // MARK: - FreezeTip()
    public void FreezeTip(TipBuilder tipBuilder)
    {
        if (!TryGetLastTip(out TipBox tip)) return;
        tipBuilder.Freeze = true;

        // 多个Tip出现时，再次叠加开始逐个将前一个Tip变暗
        if (_tipBoxes.Count - 1 > 0)
        {
            _tipBoxes[^2].Modulate = new Color(1, 1, 1, .5f);
            // 禁用上一段文本的Tip功能
            _tipBoxes[^2].MouseFilter = MouseFilterEnum.Ignore;
        }

        tip.Freeze = true;
        _darkMask.Visible = true;

        // 当鼠标点击任意空白区域
        tip.BindMouseExit((() =>
        {
            if (!TryGetLastTip(out TipBox preTip)) return;
            if (tip == preTip) UnFreezeTip(tipBuilder);
        }));
    }

    // TODO:UnFreezeTipBox()和CloseDarkMask()，可能是一个潜在的bug点，当游戏帧数出现波动时这些功能的结果是无法预测的。
    // MARK: - UnFreezeTipBox()
    public async void UnFreezeTipBox(TipBox tip)
    {
        // 由于整个程序响应过快会连续读到鼠标按下，故设置延迟避免连续操作
        // 10ms可以让鼠标刚好接上悬停响应，100ms则会直接忽略所有鼠标事件
        await Task.Delay(10);
        // ReSharper disable once RedundantArgumentDefaultValue
        tip.Modulate = new Color(1, 1, 1, 1.0f);
        // 启用上一段文本的Tip功能
        tip.MouseFilter = MouseFilterEnum.Stop;
    }

    public Action OnAllTipClose;

    // MARK: - UnFreezeTip()
    public void UnFreezeTip(TipBuilder tipBuilder = null)
    {
        if (!TryGetLastTip(out TipBox lastTip) && lastTip == null) return;

        // 当多个Tip出现时，再次叠加开始时会逐个将前一个Tip变暗，此行用于实现相反的功能
        if (_tipBoxes.Count - 1 > 0)
        {
            UnFreezeTipBox(_tipBoxes[^2]);
        }

        _tipBoxes.RemoveAt(_tipBoxes.Count - 1);
        RemoveChild(lastTip);
        lastTip.QueueFree();
        _asyncTip = false;

        // 当场上没有被冻结的Tip时，隐藏暗幕
        if (_tipBoxes.Count <= 0)
        {
            CloseDarkMask();
        }

        if (tipBuilder != null)
            tipBuilder.Freeze = false;

        // 避免在Tick中关了没重新显示，同时避免第二行Tip错误地更新名称框的逻辑
        if (_tipBoxes.Count != 0) return;
    }
    
    // MARK: - RemoveAllTip()
    public void RemoveAllTip()
    {
        _asyncTip = false;
        foreach (var tipBox in _tipBoxes)
        {
            tipBox.Destroy();
        }

        _tipBoxes.Clear();
        CloseDarkMask();

        // 由于直接删除丢失了Freeze层级的操作，最底层的文本显示区会被无限冻结，所以在删除所有Tip后为底层文本解冻
        // foreach(var node in DlgInterface.DlgTextList.GetChildren())
        // {
        //     if (node is TipBuilder tipBuilder)
        //     {
        //         tipBuilder.Freeze = false;
        //     }
        // }
    }

    public void RemoveTip(string key)
    {
        int i = 0;
        foreach (var tip in _tipBoxes.ToList())
        {
            if (tip.Key == key)
            {
                if (i < _tipBoxes.Count)
                {
                    _tipBoxes.RemoveAt(i);
                    tip.QueueFree();
                }
            }
            else
            {
                RemoveAllTip();
                break;
            }
            i++;
        }
    }
    
    // MARK: - RemoveLastTip()
    public void RemoveLastTip()
    {
        if (!TryGetLastTip(out TipBox lastTip) && lastTip == null) return;
        _tipBoxes.RemoveAt(_tipBoxes.Count - 1);
        RemoveChild(lastTip);
        lastTip.Destroy();
        _asyncTip = false;
        // 避免在Tick中关了没重新显示，同时避免第二行Tip错误地更新名称框的逻辑
        if (_tipBoxes.Count != 0) return;
    }

    public void RemoveLastTip(TipBuilder tipBuilder)
    {
        if (!TryGetLastTip(out TipBox lastTip) && lastTip == null) return;

        _tipBoxes.RemoveAt(_tipBoxes.Count - 1);
        RemoveChild(lastTip);
        lastTip.Destroy();
        _asyncTip = false;
        tipBuilder.Freeze = false;

        // 避免在Tick中关了没重新显示，同时避免第二行Tip错误地更新名称框的逻辑
        if (_tipBoxes.Count != 0) return;
    }

    // MARK: - CloseDarkMask()
    public async void CloseDarkMask()
    {
        // 同UnFreezeTipBox()，由于整个程序响应过快会，黑幕关掉的时候还能到鼠标按下，故设置延迟避免连续操作
        // 10ms可以让鼠标刚好接上悬停响应，100ms则会直接忽略所有鼠标事件
        await Task.Delay(10);
        _darkMask.Visible = false;

        OnAllTipClose?.Invoke();
    }
    #endregion

    public void Process()
    {
        if (_asyncTip)
        {
            if (TryGetLastTip(out TipBox lastTip))
            {
                var h = 0f;
                for (var index = 0; index < _tipBoxes.Count - 1; index++)
                {
                    var tipBox = _tipBoxes[index];

                    h += tipBox.Size.Y + 5;
                }

                // 当玩家在对话记录界面时，我们不需要限制Tip的位置
                if (!_lockTipPos)
                {
                    if (lastTip.Freeze)
                    {
                    }
                    else
                    {
                        Vector2 tipSize = lastTip.Size;
                        var mousePos = Game.MousePos + new Vector2(10, 0) - new Vector2(0, tipSize.Y - 35);

                        // 确定Tip最后的位置
                        var clampedPosition = new Vector2(
                            Mathf.Clamp(mousePos.X, 10, Size.X - tipSize.X - 10),
                            Mathf.Clamp(mousePos.Y, 10, Size.Y - tipSize.Y + 5)
                        );
                        // 实现
                        lastTip.Position = clampedPosition;
                    }
                }
                else
                {
                    if (lastTip.Freeze)
                    {
                    }
                    else
                    {
                        Vector2 tipSize = lastTip.Size;
                        var mousePos = Game.MousePos /* + new Vector2(10, 0) - new Vector2(0, tipSize.Y + 5)*/;

                        // 如果Tip框接近名称框则隐藏
                        //if (mousePos.X <= _nameZone.Size.X + 10)
                        //	_nameZone.Modulate = new Color(1, 1, 1, 0);
                        //else if(!_hideNameZone)
                        //	_nameZone.Modulate = new Color(1, 1, 1);

                        // 确定Tip最后的位置
                        float clampedX = Mathf.Clamp(mousePos.X,
                            _tipZone.Position.X,
                            _tipZone.Position.X + Mathf.Max(0, _tipZone.Size.X - tipSize.X));

                        float clampedY = Mathf.Clamp(mousePos.Y,
                            _tipZone.Position.Y - h,
                            _tipZone.Position.Y - h + Mathf.Max(0, _tipZone.Size.Y - tipSize.Y));

                        Vector2 clampedPosition = new Vector2(clampedX, clampedY);

                        // 实现
                        lastTip.Position = clampedPosition;
                    }
                }
            }
        }
    }
}