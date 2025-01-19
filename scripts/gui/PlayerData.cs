using Godot;

namespace Jam.scripts.gui;

public partial class PlayerData : Control , IUi
{
    [Export] private RichTextLabel Bio { get; set; }
    [Export] private RichTextLabel Psy { get; set; }
    [Export] private RichTextLabel Soc { get; set; }
    [Export] private RichTextLabel Self { get; set; }
    [Export] private RichTextLabel Day { get; set; }
    [Export] private RichTextLabel Loop { get; set; }
    
    public EUIState State { get; set; }
    public void Init()
    {
        Bio.Text = Game.PlayerData.BioPrism.GetRomanNumber();
        Psy.Text = Game.PlayerData.PsyPrism.GetRomanNumber();
        Soc.Text = Game.PlayerData.SocPrism.GetRomanNumber();
        Self.Text = Game.PlayerData.SelfPrism.GetRomanNumber();
        Day.Text = Game.PlayerData.Day.ToString();
        Loop.Text = Game.PlayerData.DeathNum.ToString();
    }
}