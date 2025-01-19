using System;
using System.Threading.Tasks;
using Godot;
using Godot.Collections;

namespace Jam.scripts.gui;

public partial class Event  : Control , IUi
{
    /// <summary>
    /// 事件名称
    /// </summary>
    [Export] private RichTextLabel _eventNameLabel;
    
    /// <summary>
    /// 事件细节
    /// </summary>
    [Export] private RichTextLabel _eventDeLabel;
    
    [Export] private AnimationPlayer _uiAnimationPlayer;
    [Export] private AnimationPlayerPlus _playerIllustrantionAnimationPlayer;
    [Export] private AnimationPlayerPlus _npcIllustrantionAnimationPlayer;
    
    [Export] private TextureRect _playerSprite; 
    [Export] private TextureRect _npcSprite;
    
    public EUIState State { get; set; }
    
    
    private bool _isEvent;
    
    public void Init()
    {
        _uiAnimationPlayer.Play("RESET");
        _playerIllustrantionAnimationPlayer.Play("RESET");
        _npcIllustrantionAnimationPlayer.Play("RESET");
    }

    /// <summary>
    /// 立绘缓存
    /// </summary>
    public Dictionary<string, CompressedTexture2D> IllData = new();

    public async Task HideIllustration(string target)
    {
        switch (target)
        {
            case "player":
                await _playerIllustrantionAnimationPlayer.PlayAsync("player_ill/Hide");
                break;
            case "npc":
                await _playerIllustrantionAnimationPlayer.PlayAsync("npc_ill/Hide");
                break;
        }
    }

    /// <summary>
    /// 显示立绘
    /// </summary>
    /// <param name="target">图层</param>
    /// <param name="path">立绘资源路径</param>
    /// <param name="onFinish">完成时</param>
    public void ShowIllustration(string target, string path, Action onFinish)
    {
        // 从缓存中查找立绘，如果没有缓存则尝试从资源里加载精灵图
        // CompressedTexture2D texture;
        /*texture = IllData.TryGetValue(path, out var ill) ? ill :
            ResourceLoader.Load<CompressedTexture2D>($"res://texture/char/{path}.png");*/
        
        // if(IllData.TryGetValue(path, out var ill))
        // {
        //     texture = ill;
        // }
        // else
        // {
        //     texture =  ResourceLoader.Load<CompressedTexture2D>($"res://texture/char/{path}.png");
        //     if(texture == null) return;
        //     // 缓存立绘
        //     IllData.Add(path, texture);
        // }
        
        // 一坨简单粗暴的状态机
        switch (target)
        {
            case "player":
                // _playerSprite.Texture = texture;
                _playerIllustrantionAnimationPlayer.Play("player_ill/Show");
                
                void OnPlayerShowFinish(StringName s)
                {
                    onFinish?.Invoke();
                    _playerIllustrantionAnimationPlayer.AnimationFinished -= OnPlayerShowFinish;
                }

                _playerIllustrantionAnimationPlayer.AnimationFinished += OnPlayerShowFinish;
                break;
            case "npc":
                //_npcSprite.Texture = texture;
                _npcIllustrantionAnimationPlayer.Play("npc_ill/Show");
                
                void OnNpcShowFinish(StringName s)
                {
                    onFinish?.Invoke();
                    _npcIllustrantionAnimationPlayer.AnimationFinished -= OnNpcShowFinish;
                }

                _npcIllustrantionAnimationPlayer.AnimationFinished += OnNpcShowFinish;
                break;
        }
    }
    
    public void ShowEvent(string eventName,string eventDe,Action onFinish)
    {
        if (_isEvent)
        {
            _uiAnimationPlayer.Play("event/Hide");
            
            void OnHideFinish(StringName s)
            {
                ShowAnima();
                _uiAnimationPlayer.AnimationFinished -= OnHideFinish;
            }
            
            _uiAnimationPlayer.AnimationFinished += OnHideFinish;
        }
        else
        {
            ShowAnima();
        }

        return;
        
        void ShowAnima()
        {
            _eventNameLabel.Text = eventName;
            _eventDeLabel.Text = eventDe;
            _isEvent = true;
        
            void OnShowFinish(StringName s)
            {
                onFinish?.Invoke();
                _uiAnimationPlayer.AnimationFinished -= OnShowFinish;
            }
            
            _uiAnimationPlayer.Play("event/Show");
            _uiAnimationPlayer.AnimationFinished += OnShowFinish;
        }
    }
}