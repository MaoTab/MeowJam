using Godot;

namespace Jam.scripts.gui;

public partial class PlayerData : Control , IUi
{
    [Export] private PrismDataBar BioBar { get; set; }
    [Export] private PrismDataBar PsyBar { get; set; }
    [Export] private PrismDataBar SocBar { get; set; }
    [Export] private PrismDataBar SelfBar { get; set; }
    
    public EUIState State { get; set; }
    public void Init()
    {
        BioBar.ChangeBarAsync((float)Game.PlayerData.BioPrism.Level / 4);
        PsyBar.ChangeBarAsync((float)Game.PlayerData.PsyPrism.Level / 4);
        SocBar.ChangeBarAsync((float)Game.PlayerData.SocPrism.Level / 4);
        SelfBar.ChangeBarAsync((float)Game.PlayerData.SelfPrism.Level / 4);
    }

    public void RefreshBio()
    {
        BioBar.ChangeBarAsync((float)Game.PlayerData.BioPrism.Level / 4);
    }

    public void RefreshPsy()
    {
        PsyBar.ChangeBarAsync((float)Game.PlayerData.PsyPrism.Level / 4);
    }

    public void RefreshSoc()
    {
        SocBar.ChangeBarAsync((float)Game.PlayerData.SocPrism.Level / 4);
    }

    public void RefreshSelf()
    {
        SelfBar.ChangeBarAsync((float)Game.PlayerData.SelfPrism.Level / 4);
    }
}