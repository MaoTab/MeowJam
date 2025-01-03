using System;
using Godot;

namespace Jam;

public partial class TipBox : Control
{
    [Export] public Control VBox;
    public string Key;
    public bool Freeze;

    public Action OnMouseInsideTip;
    public void BindMouseExit(Action onMouseInsideTip)
    {
        OnMouseInsideTip = onMouseInsideTip;
        GameEvent.OnMouseLeftDown += MouseExitEvent;
    }

    public void MouseExitEvent(Vector2 mousePos)
    {
        // 判断鼠标点击位置是否在tip的范围内
        bool isMouseInsideTip = mousePos.X >= Position.X &&
                                mousePos.X <= Position.X + Size.X &&
                                mousePos.Y >= Position.Y &&
                                mousePos.Y <= Position.Y + Size.Y;

        if (!isMouseInsideTip)
        {
            OnMouseInsideTip.Invoke();
        }
    }
    
    public override void _ExitTree() 
    {
        // 取消事件订阅
        GameEvent.OnMouseLeftDown -= MouseExitEvent;
    }
}