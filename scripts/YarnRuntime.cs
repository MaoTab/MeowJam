using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Godot;
using YarnSpinnerGodot;

namespace Jam;

public class YarnRuntime
{
    private DialogueRunner _dialogueRunner;
    
    // MARK: - PlayNode()
    public void PlayNode(string node)
    {
        Game.Gui.DlgInterface.OnAnimationFinish += OnFinish;
        Game.Gui.DlgInterface.Show();
        return;

        void OnFinish()
        {
            _dialogueRunner.Dialogue.SetNode(node);
            _dialogueRunner.ContinueDialogue();
            Game.Gui.DlgInterface.OnAnimationFinish -= OnFinish;
        }
    }
    
    // MARK: - Stop()
    public void Stop()
    {
        _dialogueRunner.Dialogue.Stop();
        // _dialogueRunner.Stop();
    }

    // MARK: - Init()
    public YarnRuntime Init(YarnProject project)
    {
        _dialogueRunner = new DialogueRunner();
        _dialogueRunner.SetProject(project);
        _dialogueRunner.OnHandleOptions += OnDialogueRunnerOnHandleOptions;
        _dialogueRunner.OnHandleLine += OnDialogueRunnerOnHandleLine;

        _dialogueRunner.onDialogueComplete += () =>
        {
            GD.Print("Dialogue complete");
        };

        _dialogueRunner.onNodeComplete += _ =>
        {
            GD.Print("Node complete");
        };
        
        // MARK: - dead
        _dialogueRunner.AddCommandHandler("dead",(() =>
        {
            
            _dialogueRunner.ContinueDialogue();
        }));
        
        // MARK: - dlg_mode
        _dialogueRunner.AddCommandHandler<string>("dlg_mode",((mode) =>
        {
            switch (mode)
            {
                case "perception":
                    Game.Gui.DlgInterface.AddSeparator("感知");
                    break;
                case "discussion":
                    Game.Gui.DlgInterface.AddSeparator("讨论");
                    break;
                case "do":
                    Game.Gui.DlgInterface.AddSeparator("执行");
                    break;
            }
            
            _dialogueRunner.ContinueDialogue();
        }));
        
        // MARK: - clear_dlg
        _dialogueRunner.AddCommandHandler("clear_dlg",(() =>
        {
            Game.Gui.DlgInterface.RemoveAllDlg();
            _dialogueRunner.ContinueDialogue();
        }));
        
        // MARK: - show_ill
        _dialogueRunner.AddCommandHandler<string,string>(("show_ill"), (target,path) =>
        {
            Game.Gui.Event.ShowIllustration(target, path , () =>
            {
                _dialogueRunner.ContinueDialogue();
            });
        });
        
        // MARK: - move_to
        _dialogueRunner.AddCommandHandler<int,int>(("move_to"), (x,y) =>
        {
            _ =  Game.Gui.DlgInterface.AddDlgText("", "[wave]移动中...[/wave]", null);
            
            
            Game.ControlRole.MoveTo(new Vector2(x,y), () =>
            {
                Game.Gui.DlgInterface.RemovePreDlg();
                _dialogueRunner.ContinueDialogue();
            });
        });
        
        // MARK: - move_to_node
        _dialogueRunner.AddCommandHandler<string>(("move_to_node"), (name) =>
        {
            if (Game.Level.LevelNodes.TryGetValue(name, out var node))
            {
                _ = Game.Gui.DlgInterface.AddDlgText("", "[wave]移动中...[/wave]", null);
                
                Game.ControlRole.MoveTo(node.Position,() =>
                {
                    Game.Gui.DlgInterface.RemovePreDlg();
                    _dialogueRunner.ContinueDialogue();
                });
            }
            else
            {
                _dialogueRunner.ContinueDialogue();
            }
        });
        
        // MARK: - show_event
        _dialogueRunner.AddCommandHandler<string,string>(("show_event"), (name,de) =>
        {
            Game.Gui.Event.ShowEvent(name,de,(() =>
            {
                _dialogueRunner.ContinueDialogue();
            }));
        });
        
        // MARK: - vote
        _dialogueRunner.AddCommandHandler("vote",(() =>
        {
            Game.Gui.Vote.Show();
            _dialogueRunner.ContinueDialogue();
        }));
        
        // MARK: - vote_over
        _dialogueRunner.AddCommandHandler("vote_over",(() =>
        {
            Game.Gui.Vote.Hide();
            _dialogueRunner.ContinueDialogue();
        }));
        
        // MARK: - prism_get
        _dialogueRunner.AddFunction<string,int>("prism_get",((name) =>
        {
            return name switch
            {
                "psy" => Game.PlayerData.PsyPrism.Level,
                "soc" => Game.PlayerData.SocPrism.Level,
                "self" => Game.PlayerData.SelfPrism.Level,
                "bio" => Game.PlayerData.BioPrism.Level,
                _ => 0
            };
        }));
        
        // MARK: - prism_add
        _dialogueRunner.AddCommandHandler<string,int>("prism_add",((name,level) =>
        {
            switch (name)
            {
                case "psy":
                    Game.PlayerData.PsyPrism.Level += level;
                    Game.Gui.PlayerData.RefreshPsy();
                    break;
                case "soc":
                    Game.PlayerData.SocPrism.Level += level;
                    Game.Gui.PlayerData.RefreshSoc();
                    break;
                case "self":
                    Game.PlayerData.SelfPrism.Level += level;
                    Game.Gui.PlayerData.RefreshSelf();
                    break;
                case "bio":
                    Game.PlayerData.BioPrism.Level += level;
                    Game.Gui.PlayerData.RefreshBio();
                    break;
            }
            
            _dialogueRunner.ContinueDialogue();
        }));

        // MARK: - save
        _dialogueRunner.AddCommandHandler("save", () =>
        {
            Game.PlayerData.Save();
            _dialogueRunner.ContinueDialogue();
        });
        
        // MARK: - end
        _dialogueRunner.AddCommandHandler("end",(() =>
        {
            Game.Gui.DlgInterface.Hide();
            
            Game.Gui.DlgInterface.RemoveAllDlg();
            
            Game.CanControl = true;
        }));
        
        // MARK: - OnDialogueRunnerOnHandleOptions()
        // 对标准行的处理
        async void OnDialogueRunnerOnHandleLine(LocalizedLine localizedLine)
        {
            var line = localizedLine.TextWithoutCharacterName.Text;
            var charName = localizedLine.CharacterName;
            var skipContinue = false;

            foreach (var attribute in localizedLine.TextWithoutCharacterName.Attributes)
            {
                switch (attribute.Name)
                {
                    case "continue":
                        skipContinue = true;
                        break;
                }
            }

#pragma warning disable VSTHRD101
            await Game.Gui.DlgInterface.AddDlgText(charName,line, async void () =>
            {
                if (skipContinue)
                {
                    _dialogueRunner.ContinueDialogue();
                }
                else
                {
                    await Game.Gui.DlgInterface.AddOption("继续", () =>
                        {
                            Game.Gui.DlgInterface.RemoveAllOption();
                            _dialogueRunner.ContinueDialogue();
                        }, 
                        //() => HandleTipDisplay(true), // OnEnter
                        //() => HandleTipDisplay(false), // OnExit
                        () => {}, 
                        () => {},
                        () => {},10);
                }
            });
#pragma warning restore VSTHRD101
        }

        // MARK: - OnDialogueRunnerOnHandleOptions()
        // 对选项的处理
        async void OnDialogueRunnerOnHandleOptions(DialogueOption[] options)
        {
            // 遍历选项
            foreach (var option in options)
            {
                if (!option.IsAvailable) continue; // 选项不激活则跳过

                Action onEnter = () => {};
                Action onExit = () => {};
                
                var line = option.Line.TextWithoutCharacterName.Text;
                var hasTip = false;
                List<string> tipList;
                bool isTipHeld;

                foreach (var attribute in option.Line.TextWithoutCharacterName.Attributes)
                {
                    switch (attribute.Name)
                    {
                        case "vote":
                            if (attribute.Properties.TryGetValue("t", out var property))
                            {
                                switch (property.StringValue)
                                {
                                    case "psy":
                                        onEnter += () => Game.Gui.Vote.ViewVotingResults(0, Game.PlayerData.SelfPrism.Level, 0);
                                          break;
                                    case "soc":
                                        onEnter += () => Game.Gui.Vote.ViewVotingResults(0, 0, Game.PlayerData.SelfPrism.Level);
                                        break;
                                    case "bio":
                                        onEnter += () => Game.Gui.Vote.ViewVotingResults(Game.PlayerData.SelfPrism.Level, 0, 0);
                                        break;
                                }
                                
                                onExit += () => Game.Gui.Vote.Normal();
                            }
                            break;
                    }
                }
                
                async void OnClick()
                {
                    Game.Gui.DlgInterface.RemoveAllOption();
                    
                    await Game.Gui.DlgInterface.AddDlgText("Player",line, () =>
                    {
                        _dialogueRunner.SelectedOption(option.DialogueOptionID);
                    } );
                    //
                    // _dialogueRunner.SelectedOption(option.DialogueOptionID);
                }
                
#pragma warning disable CS8321 // 已声明本地函数，但从未使用过
                void HandleTipDisplay(bool isEntering)
#pragma warning restore CS8321 // 已声明本地函数，但从未使用过
                {
                    if (!hasTip) return;
                    if (isEntering)
                    {
                        Game.Gui.CreatListTip(tipList, true);
                    }
                    else if (!isTipHeld)
                    {
                        Game.Gui.RemoveLastTip();
                    }
                }
                
                void OnHold()
                {
                    // ReSharper disable once ConditionIsAlwaysTrueOrFalse
                    if (!hasTip) return;
                    isTipHeld = true;
                    Game.Gui.FreezeTipFromButton();
                }
                
                await Task.Delay(50); // 增加一些延迟，避免文本还没显示完就开始弹选项了（实际上是文本显示完一瞬间就开始弹选项造成的错觉）
                await Game.Gui.DlgInterface.AddOption(line, OnClick, 
                    //() => HandleTipDisplay(true), // OnEnter
                    //() => HandleTipDisplay(false), // OnExit
                    () => { onEnter.Invoke(); }, 
                    () => { onExit.Invoke();},
                    OnHold,10);
            }
        }
        
        return this;
    }
    
    
    // MARK: - Start()
    public void Start()
    {
        _dialogueRunner.startNode = "Node_NULL";
        _dialogueRunner.startAutomatically = false;
        _dialogueRunner.Init(); // 初始化并启动Yarn
    }
    
    # if !DEBUG
    [YarnFunction("prism_get")]
    public int prism_get(string node)
    {
        return 0;
    }

    [YarnCommand("show_event")]
    public void 显示事件(string 事件标题,string 事件描述)
    {
    }

    [YarnCommand("show_ill")]
    public void 显示立绘(string 目标,string 立绘路径)
    {
    }

    [YarnCommand("move_to")]
    public void 移动至坐标位置(int x,int y)
    {
    }
    
    [YarnCommand("clear_dlg")]
    public void 清空对话列表()
    {
    }
    #endif
}