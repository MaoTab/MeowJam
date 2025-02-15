﻿using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Godot;
using YarnSpinnerGodot;

namespace SnowBlindness;

public class YarnRuntime
{
    private DialogueRunner _dialogueRunner;

    // MARK: - PlayNode()
    public void PlayNode(string node)
    {
       
        _dialogueRunner.Dialogue.SetNode(node);
        _dialogueRunner.ContinueDialogue();
    }

    // MARK: - Stop()
    public void Stop()
    {
        _dialogueRunner.Dialogue.Stop();
        // _dialogueRunner.Stop();
    }

    public bool checkResults;
    
    // MARK: - Init()
    public YarnRuntime Init(YarnProject project)
    {
        _dialogueRunner = new DialogueRunner();
        _dialogueRunner.SetProject(project);
        _dialogueRunner.OnHandleOptions += OnDialogueRunnerOnHandleOptions;
        _dialogueRunner.OnHandleLine += OnDialogueRunnerOnHandleLine;

        _dialogueRunner.onDialogueComplete += () => { GD.Print("Dialogue complete"); };

        _dialogueRunner.onNodeComplete += _ => { GD.Print("Node complete"); };
        
        
        // MARK: - black
        _dialogueRunner.AddCommandHandler("black", (async () =>
        {
            await Game.Gui.AnimationPlayer.PlayAsync("black/Show");
            _dialogueRunner.ContinueDialogue();
            await Game.Gui.AnimationPlayer.PlayAsync("black/Hide");
        }));
            
        // MARK: - audio
        _dialogueRunner.AddCommandHandler<string>("audio", ((path) =>
        {
            switch (path)
            {
                case "deng_shan":
                    Game.Audio.PlayDengShan();
                    break;
            }
          
            _dialogueRunner.ContinueDialogue();
        }));
            
        // MARK: - audio_stop
        _dialogueRunner.AddCommandHandler<string>("audio_stop", ((path) =>
        {
            switch (path)
            {
                case "deng_shan":
                    Game.Audio.StopDengShan();
                    break;
            }
            
            _dialogueRunner.ContinueDialogue();
        }));
        
        // MARK: - check
        _dialogueRunner.AddCommandHandler<string,int>("check", (async (target,dc) =>
        {
            checkResults = target switch
            {
                "soc" => await Game.Gui.DlgInterface.Check(Game.PlayerData.SocPrism.Level, dc),
                "bio" => await Game.Gui.DlgInterface.Check(Game.PlayerData.BioPrism.Level, dc),
                "psy" => await Game.Gui.DlgInterface.Check(Game.PlayerData.PsyPrism.Level, dc),
                "self" => await Game.Gui.DlgInterface.Check(Game.PlayerData.SelfPrism.Level, dc),
                _ => false
            };
            
            _dialogueRunner.ContinueDialogue();
        }));

        // MARK: - check_get
        _dialogueRunner.AddFunction<bool>("check_get", (() =>
        {
            return checkResults;
        }));
        
        // MARK: - dlg_mode
        _dialogueRunner.AddCommandHandler<string>("dlg_mode", ((mode) =>
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
        _dialogueRunner.AddCommandHandler("clear_dlg", (() =>
        {
            Game.Gui.DlgInterface.RemoveAllDlg();
            _dialogueRunner.ContinueDialogue();
        }));

        // MARK: - show_ill
        _dialogueRunner.AddCommandHandler<string, string>(("show_ill"),
            (target, path) =>
            {
                Game.Gui.Event.ShowIllustration(target, path, () => { _dialogueRunner.ContinueDialogue(); });
            });

        // MARK: - hide_ill
        _dialogueRunner.AddCommandHandler<string>(("hide_ill"), async (target) =>
        {
           await Game.Gui.Event.HideIllustration(target);
           _dialogueRunner.ContinueDialogue();
        });
        
        // MARK: - move_to
        _dialogueRunner.AddCommandHandler<int, int>(("move_to"), (x, y) =>
        {
            _ = Game.Gui.DlgInterface.AddDlgText("", "[wave]移动中...[/wave]", null);
            
            Game.ControlRole.MoveTo(new Vector2(x, y), () =>
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

                Game.ControlRole.MoveTo(node.Position, () =>
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
        _dialogueRunner.AddCommandHandler<string, string>(("show_event"),
            (name, de) => { Game.Gui.Event.ShowEvent(name, de, (() => { _dialogueRunner.ContinueDialogue(); })); });

        // MARK: - vote
        _dialogueRunner.AddCommandHandler("vote", (() =>
        {
            Game.Gui.Vote.Show();
            _dialogueRunner.ContinueDialogue();
        }));

        // MARK: - vote_over
        _dialogueRunner.AddCommandHandler<string>("vote_over", (async (target) =>
        {
            Game.Gui.Vote.Hide();
            
            switch (target)
            {
                case "bio":
                    Game.Gui.Vote.VoteResTexture.Texture = Game.Gui.Vote.BioVoteResTexture;
                    Game.Gui.Vote.VoteResCanvas.Modulate = new Color(.61f, .28f, .24f);
                    break;
                case "psy":
                    Game.Gui.Vote.VoteResTexture.Texture = Game.Gui.Vote.PsyVoteResTexture;
                    Game.Gui.Vote.VoteResCanvas.Modulate = new Color(.21f, .32f, .42f);
                    break;
                case "soc":
                    Game.Gui.Vote.VoteResTexture.Texture = Game.Gui.Vote.SocVoteResTexture;
                    Game.Gui.Vote.VoteResCanvas.Modulate = new Color(.30f, .43f, .40f);
                    break;
                default:
                    return;
            } 
            
            await Game.Gui.Vote.VoteResAnimationPlayer.PlayAsync("all_gui/vote_over");
            _dialogueRunner.ContinueDialogue();
        }));

        // MARK: - prism_get
        _dialogueRunner.AddFunction<string, int>("prism_get", ((name) =>
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

        // MARK: - prism_vote
        _dialogueRunner.AddFunction<string, string>("prism_vote", ((name) =>
        {
            var psy = Game.PlayerData.PsyPrism.Level;
            var soc = Game.PlayerData.SocPrism.Level;
            var bio = Game.PlayerData.BioPrism.Level;

            var play = Game.PlayerData.SelfPrism.Level;
            
            int maxLevel = play;
            string maxPerson = "self";

            // 找到三个支持人员中的最大等级
            if (psy > maxLevel)
            {
                maxLevel = psy;
                maxPerson = "psy";
            }
            if (soc > maxLevel)
            {
                maxLevel = soc;
                maxPerson = "soc";
            }
            if (bio > maxLevel)
            {
                maxLevel = bio;
                maxPerson = "bio";
            }

            // 根据玩家支持的人员计算新的等级
            switch (name)
            {
                case "psy":
                    var psyTotal = psy + play;
                    if (psyTotal > maxLevel)
                    {
                        maxPerson = "psy";
                    }
                    break;
                case "soc":
                    var socTotal = soc + play;
                    if (socTotal > maxLevel)
                    {
                        maxPerson = "soc";
                    }
                    break;
                case "bio":
                    var bioTotal = bio + play;
                    if (bioTotal > maxLevel)
                    {
                        maxPerson = "bio";
                    }
                    break;
            }

            // 返回等级最高的人员
            return maxPerson;
        }));
            
        
        // MARK: - prism_add
        _dialogueRunner.AddCommandHandler<string, int>("prism_add", ((name, level) =>
        {
            switch (name)
            {
                case "psy":
                    Game.PlayerData.PsyPrism.Level += level;
                    break;
                case "soc":
                    Game.PlayerData.SocPrism.Level += level;
                    break;
                case "self":
                    Game.PlayerData.SelfPrism.Level += level;
                    break;
                case "bio":
                    Game.PlayerData.BioPrism.Level += level;
                    break;
            }

            Game.Gui.PlayerData.Init();
            
            _dialogueRunner.ContinueDialogue();
        }));

        // MARK: - save
        _dialogueRunner.AddCommandHandler("save", () =>
        {
            Game.PlayerData.Save();
            _dialogueRunner.ContinueDialogue();
        });

        // MARK: - day_set
        _dialogueRunner.AddCommandHandler<int>("day_set", ((num) =>
        {
            Game.PlayerData.Day = num;
            Game.Gui.PlayerData.Init();
            _dialogueRunner.ContinueDialogue();
        }));
        
        // MARK: - dead_wait
        _dialogueRunner.AddCommandHandler("dead_wait", (async () =>
        {
            Game.PlayerData.DeathNum++;
            Game.Gui.PlayerData.Init();
            await Game.Gui.Death.Show();
            _dialogueRunner.ContinueDialogue();
        }));
        
        // MARK: - dead
        _dialogueRunner.AddCommandHandler("dead", (() =>
        {
            _ = Game.Gui.Death.Show();
            Game.PlayerData.DeathNum++;
            Game.Gui.PlayerData.Init();
            _dialogueRunner.ContinueDialogue();
        }));
        
        // MARK: - end
        _dialogueRunner.AddCommandHandler("end", (() =>
        {
            Game.Gui.Death.AnimationPlayer.Play("full_ui/thank");
        }));

        // MARK: - OnDialogueRunnerOnHandleOptions()
        // 对标准行的处理
        async void OnDialogueRunnerOnHandleLine(LocalizedLine localizedLine)
        {
            var line = localizedLine.TextWithoutCharacterName.Text;
            var charName = localizedLine.CharacterName;
            var skipContinue = false;

            // GD.Print(localizedLine.LineResource.);
            
            foreach (var attribute in localizedLine.TextWithoutCharacterName.Attributes)
            {
                switch (attribute.Name)
                {
                    case "continue":
                        skipContinue = true;
                        break;
                    case "skip":
                        if (attribute.Properties.TryGetValue("b", out var bProperty))
                        {
                            if (bProperty.ToString() == "True")
                            {
                                _dialogueRunner.ContinueDialogue();
                                return;
                            }
                        }
                        break;
                }
            }
            
#pragma warning disable VSTHRD101
            await Game.Gui.DlgInterface.AddDlgText(charName, line, async void () =>
            {
                GD.Print(charName + line);
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
                        () => { },
                        () => { },
                        () => { }, 10);
                }
            });
#pragma warning restore VSTHRD101
        }

        // MARK: - OnDialogueRunnerOnHandleOptions()
        // 对选项的处理
        async void OnDialogueRunnerOnHandleOptions(DialogueOption[] options)
        {
            Game.Gui.DlgInterface.AddOption("NULL", () =>
                {
                    Game.Gui.DlgInterface.RemoveAllOption();
                    _dialogueRunner.ContinueDialogue();
                },
                //() => HandleTipDisplay(true), // OnEnter
                //() => HandleTipDisplay(false), // OnExit
                () => { },
                () => { },
                () => { }, 10,true);
            
            // 遍历选项
            foreach (var option in options)
            {
                if (!option.IsAvailable) continue; // 选项不激活则跳过

                Action onEnter = () => { };
                Action onExit = () => { };
                
                var line = option.Line.TextWithoutCharacterName.Text;
                line = Regex.Replace(line, "</n>", "\n");
                var hasTip = false;

                // 用于显示检定内容
                var hasCheck = false;
                var tipMainContent = "";
                var tipChiContent = "";
                var dc = 0;
                var i = 0; //用于加值显示
                var ii = 0; //用于减值显示
                var checkTarget = ""; //用于减值显示

                List<string> tipList = new List<string>();
                bool isTipHeld;

                foreach (var attribute in option.Line.TextWithoutCharacterName.Attributes)
                {
                    switch (attribute.Name)
                    {
                        case "tip":
                            if (attribute.Properties.TryGetValue("s", out var sProperty))
                            {
                                tipChiContent = sProperty.ToString();
                            }
                            break;
                        case "check":
                            if (attribute.Properties.TryGetValue("dc", out var dcProperty))
                            {
                                dc = dcProperty.IntegerValue;
                                var checkName = "Check_1";
                                hasTip = true;

                                if (attribute.Properties.TryGetValue("name", out var checkNameV))
                                {
                                    checkName = checkNameV.StringValue;
                                }

                                if (attribute.Properties.TryGetValue("target", out var checkTargetV))
                                {
                                    checkTarget = checkTargetV.StringValue;
                                }
                                
                                tipMainContent = "[p align=center]" + checkName + "\n" + "难度：" + dc + "[/p]";
                                hasCheck = true;
                            }

                            break;
                        case "vote":
                            if (attribute.Properties.TryGetValue("t", out var property))
                            {
                                switch (property.StringValue)
                                {
                                    case "psy":
                                        onEnter += () =>
                                            Game.Gui.Vote.ViewVotingResults(0, Game.PlayerData.SelfPrism.Level, 0);
                                        break;
                                    case "soc":
                                        onEnter += () =>
                                            Game.Gui.Vote.ViewVotingResults(0, 0, Game.PlayerData.SelfPrism.Level);
                                        break;
                                    case "bio":
                                        onEnter += () =>
                                            Game.Gui.Vote.ViewVotingResults(Game.PlayerData.SelfPrism.Level, 0, 0);
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
                    _dialogueRunner.SelectedOption(option.DialogueOptionID);
                    
                    /*await Game.Gui.DlgInterface.AddDlgText("Player", line,
                        () => { _dialogueRunner.SelectedOption(option.DialogueOptionID); });*/
                    
                    // _dialogueRunner.SelectedOption(option.DialogueOptionID);
                }

                void HandleTipDisplay(bool isEntering)
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

                // 检定内容构建部分
                if (hasCheck)
                {
                    var add = "";

                    if (i != 0)
                    {
                        add = " + " + i;
                    }

                    if (ii != 0)
                    {
                        var sub = " + " + ii;
                        tipMainContent += $"[font_size=9]{sub}({dc + ii})[/font_size]";
                    }
                    
                    var cdTarget = 0;
                    
                    switch (checkTarget)
                    {
                        case "self":
                            cdTarget = Game.PlayerData.SelfPrism.Level;
                            break;
                        case "soc":
                            cdTarget = Game.PlayerData.SocPrism.Level;
                            break;
                        case "psy":
                            cdTarget = Game.PlayerData.PsyPrism.Level;
                            break;
                        case "bio":
                            cdTarget = Game.PlayerData.BioPrism.Level;
                            break;
                    }
                    
                    var cd = MathfHelper.Calculate4D6Rate(cdTarget,dc);
                    
                    var hexColor = cd switch
                    {
                        >= 24 => "#a9dc76", // 简单 - 绿色
                        >= 18 => "#fd971f", // 普通 - 黄色
                        >= 14 => "#c25d2b", // 困难 - 橙色
                        >= 8 => "#ff6188", // 非常困难 - 红色
                        _ => "#cc3941"
                    };

                    tipList.Add(tipMainContent);

                    if (!string.IsNullOrEmpty(tipChiContent))
                    {
                        tipList.Add("line");
                        tipList.Add(tipChiContent);
                    }
                    
                    if (cdTarget >= 4)
                    {
                        tipList.Add("line");
                        tipList.Add("[color=#a9dc76]绝对成功[/color]");
                    }
                    
                    // tipList.Add("line");
                    // tipList.Add(
                    //     $"[p align=center]成功率:[color={hexColor}]{cd:P1}[/color][font_size=9]({cdTarget}d6{add})[/font_size]");

                    onEnter += () =>
                    {
                        Game.Gui.CreatListTip(tipList, true);
                    };

                    onExit += () =>
                    {
                        Game.Gui.RemoveLastTip();
                    };
                }

                await Task.Delay(50); // 增加一些延迟，避免文本还没显示完就开始弹选项了（实际上是文本显示完一瞬间就开始弹选项造成的错觉）
                await Game.Gui.DlgInterface.AddOption(line, OnClick,
                    //() => HandleTipDisplay(true), // OnEnter
                    //() => HandleTipDisplay(false), // OnExit
                    () => { onEnter.Invoke(); },
                    () => { onExit.Invoke(); },
                    OnHold, 10);
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
    
    [YarnFunction("prism_vote")]
    public string prism_vote(string node)
    {
        return "0";
    }

    [YarnCommand("show_event")]
    public void 显示事件(string 事件标题,string 事件描述)
    {
    }

    [YarnCommand("show_ill")]
    public void 显示立绘(string 目标,string 立绘路径)
    {
    }

    [YarnCommand("hide_ill")]
    public void 隐藏立绘(string 目标)
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