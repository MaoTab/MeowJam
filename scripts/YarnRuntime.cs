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
		
        if(_dialogueRunner.Dialogue.IsActive) Stop();
        _dialogueRunner.Dialogue.SetNode(node);
        _dialogueRunner.ContinueDialogue();
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
        
        _dialogueRunner.AddCommandHandler("end",(() =>
        {
            Game.Gui.DlgInterface.RemoveAllDlg();
        }));
        
        // 对标准行的处理
        async void OnDialogueRunnerOnHandleLine(LocalizedLine localizedLine)
        {
            var line = localizedLine.TextWithoutCharacterName.Text;
            await Game.Gui.DlgInterface.AddDlgText(line);
            _dialogueRunner.ContinueDialogue();
        }

        // 对选项的处理
        async void OnDialogueRunnerOnHandleOptions(DialogueOption[] options)
        {
            // 遍历选项
            foreach (var option in options)
            {
                if (!option.IsAvailable) continue; // 选项不激活则跳过
                
                var line = option.Line.TextWithoutCharacterName.Text;
                var hasTip = false;
                var tipList = new List<string>();
                bool isTipHeld = false;
                
                void OnClick()
                {
                    Game.Gui.DlgInterface.RemoveAllOption();
                    _dialogueRunner.SelectedOption(option.DialogueOptionID);
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
                    () => HandleTipDisplay(true), // OnEnter
                    () => HandleTipDisplay(false), // OnExit
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
}