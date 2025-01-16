using Godot;

namespace Jam.scripts.gui;

public partial class Vote : Control, IUi
{
    public EUIState State { get; set; }
    
    [Export] private VoteBar BioBar { get; set; }
    [Export] private VoteBar PsyBar { get; set; }
    [Export] private VoteBar SocBar { get; set; }
    
    [Export] private AnimationPlayer AnimationPlayer { get; set; }
    
    public void Init()
    {
        AnimationPlayer.AnimationFinished += name =>
        {
            switch (name)
            {
                case "vote/Show":
                    Normal();
                    State = EUIState.ShowAndIdle;
                    break;
                case "vote/Hide":
                    State = EUIState.HideAndIdle;
                    break;
            }
        };
    }

    public new void Show()
    {
        AnimationPlayer.Play("vote/Show");
    }
    
    public new void Hide()
    {
        AnimationPlayer.Play("vote/Hide");
    }
    
    public void Normal()
    {
        if(State != EUIState.ShowAndIdle) return;
        
        BioBar.ChangeBarAsync((float)Game.PlayerData.BioPrism.Level / 8);
        PsyBar.ChangeBarAsync((float)Game.PlayerData.PsyPrism.Level / 8);
        SocBar.ChangeBarAsync((float)Game.PlayerData.SocPrism.Level / 8);
    }
    
    public void ViewVotingResults(int bioPrism, int psyPrism, int socPrism)
    {
        if(State != EUIState.ShowAndIdle) return;
        
        BioBar.ChangeBarAsync((float)(Game.PlayerData.BioPrism.Level + bioPrism) / 8);
        PsyBar.ChangeBarAsync((float)(Game.PlayerData.PsyPrism.Level + psyPrism) / 8);
        SocBar.ChangeBarAsync((float)(Game.PlayerData.SocPrism.Level + socPrism) / 8);
    }
}