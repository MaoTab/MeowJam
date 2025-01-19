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
        Bio.Text = "[url=\"肉体等级\"]" + Game.PlayerData.BioPrism.GetRomanNumber();
        Psy.Text = "[url=\"理性等级\"]" + Game.PlayerData.PsyPrism.GetRomanNumber();
        Soc.Text = "[url=\"情感等级\"]" +Game.PlayerData.SocPrism.GetRomanNumber();
        Self.Text = "[url=\"神性等级\"]" +Game.PlayerData.SelfPrism.GetRomanNumber();
        Day.Text = Game.PlayerData.Day.ToString();
        Loop.Text = Game.PlayerData.DeathNum.ToString();
    }
}