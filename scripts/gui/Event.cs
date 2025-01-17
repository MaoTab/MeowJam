using System;
using Godot;

namespace Jam.scripts.gui;

public partial class Event  : Control , IUi
{
    /// <summary>
    /// 事件名称
    /// </summary>
    [Export] private RichTextLabel eventNameLabel;
    
    /// <summary>
    /// 事件细节
    /// </summary>
    [Export] private RichTextLabel eventDeLabel;
    
    [Export] private AnimationPlayer animationPlayer;
    
    public EUIState State { get; set; }
    
    
    private bool isEvent;
    
    public void Init()
    {
        animationPlayer.Play("RESET");
    }

    public void ShowEvent(string eventName,string eventDe,Action onFinish)
    {
        if (isEvent)
        {
            animationPlayer.Play("event/Hide");
            animationPlayer.AnimationFinished += OnHideFinsh;
        }
        else
        {
            ShowAnima();
        }

        return;

        void OnHideFinsh(StringName s)
        {
            ShowAnima();
            animationPlayer.AnimationFinished -= OnHideFinsh;
        }
        
        void OnShowFinsh(StringName s)
        {
            onFinish?.Invoke();
            animationPlayer.AnimationFinished -= OnShowFinsh;
        }
        
        void ShowAnima()
        {
            eventNameLabel.Text = eventName;
            eventDeLabel.Text = eventDe;
            isEvent = true;
        
            animationPlayer.Play("event/Show");
            animationPlayer.AnimationFinished += OnShowFinsh;
        }
    }
}