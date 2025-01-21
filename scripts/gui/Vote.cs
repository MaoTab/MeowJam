using Godot;

namespace SnowBlindness.scripts.gui;

public partial class Vote : Control, IUi
{
    public EUIState State { get; set; }
    
    [Export] public TextureRect VoteResTexture { get; set; }
    [Export] public CompressedTexture2D PsyVoteResTexture { get; set; }
    [Export] public CompressedTexture2D BioVoteResTexture { get; set; }
    [Export] public CompressedTexture2D SocVoteResTexture { get; set; }
    [Export] public Control VoteResCanvas { get; set; }
    [Export] private VoteBar BioBar { get; set; }
    [Export] private VoteBar PsyBar { get; set; }
    [Export] private VoteBar SocBar { get; set; }
    
    [Export] private TextureRect BioBarOk { get; set; }
    [Export] private TextureRect PsyBarOk { get; set; }
    [Export] private TextureRect SocBarOk { get; set; }
    
    [Export] public AnimationPlayerPlus VoteResAnimationPlayer { get; set; }
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
        
        var TotalStages = Game.PlayerData.PsyPrism.Level + Game.PlayerData.BioPrism.Level +
                          Game.PlayerData.SocPrism.Level;
        
        BioBarOk.Visible = false;
        PsyBarOk.Visible = false;
        SocBarOk.Visible = false;
        
        BioBar.ChangeBarAsync((float)Game.PlayerData.BioPrism.Level / TotalStages,TotalStages);
        PsyBar.ChangeBarAsync((float)Game.PlayerData.PsyPrism.Level / TotalStages,TotalStages);
        SocBar.ChangeBarAsync((float)Game.PlayerData.SocPrism.Level / TotalStages,TotalStages);
    }
    
    public void ViewVotingResults(int bioPrism, int psyPrism, int socPrism)
    {
        if(State != EUIState.ShowAndIdle) return;
        var TotalStages = Game.PlayerData.SelfPrism.Level + Game.PlayerData.PsyPrism.Level + Game.PlayerData.BioPrism.Level +
                          Game.PlayerData.SocPrism.Level;

        BioBarOk.Visible = bioPrism > 0;
        PsyBarOk.Visible = psyPrism > 0;
        SocBarOk.Visible = socPrism > 0;
        
        BioBar.ChangeBarAsync((float)(Game.PlayerData.BioPrism.Level + bioPrism) / TotalStages,TotalStages);
        PsyBar.ChangeBarAsync((float)(Game.PlayerData.PsyPrism.Level + psyPrism) / TotalStages,TotalStages);
        SocBar.ChangeBarAsync((float)(Game.PlayerData.SocPrism.Level + socPrism) / TotalStages,TotalStages);
    }
}