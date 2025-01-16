using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Godot;
using YarnSpinnerGodot;

namespace Jam;

public class YarnRuntime
{
    private DialogueRunner _dialogueRunner;
    
    public void PlayNode(string node)
    {
        // 开始对话默认锁定玩家位移
        // Game.Core.StopInput();
		
        // 开始对话默认打开对话框
        // Game.Gui.ShowDialogueZone();
		
        // if(_dialogueRunner.Dialogue.IsActive) Stop();

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
    
    public void Stop()
    {
        _dialogueRunner.Dialogue.Stop();
        // _dialogueRunner.Stop();
    }

    
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

        _dialogueRunner.onNodeComplete += name =>
        {
            GD.Print("Node complete");
        };
        
        _dialogueRunner.AddCommandHandler<int,int>(("move_to"), (x,y) =>
        {
            Game.ControlRole.MoveTo(new Vector2(x,y), () =>
            {
                _dialogueRunner.ContinueDialogue();
            });
        });
        
        _dialogueRunner.AddCommandHandler(("obs"), () =>
        {
            
        });
        
        _dialogueRunner.AddCommandHandler("vote",(() =>
        {
            Game.Gui.Vote.Show();
            _dialogueRunner.ContinueDialogue();
        }));
        
        _dialogueRunner.AddCommandHandler("vote_over",(() =>
        {
            Game.Gui.Vote.Hide();
            _dialogueRunner.ContinueDialogue();
        }));
        
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

        _dialogueRunner.AddCommandHandler("save", () =>
        {
            Game.PlayerData.Save();
            _dialogueRunner.ContinueDialogue();
        });
        
        _dialogueRunner.AddCommandHandler("end",(() =>
        {
            Game.Gui.DlgInterface.Hide();
            
            Game.Gui.DlgInterface.RemoveAllDlg();
            
            Game.CanControl = true;
        }));
        
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
                var tipList = new List<string>();
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
    #endif
}