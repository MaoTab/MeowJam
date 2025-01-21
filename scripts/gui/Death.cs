using System.Threading.Tasks;
using Godot;

namespace SnowBlindness.scripts.gui;

public partial class Death : Control , IUi
{
    public EUIState State { get; set; }
    public void Init()
    {
        
    }
    
    [Export] public AnimationPlayerPlus AnimationPlayer { get; set; }
    
    [Export] public RichTextLabel newDeathText { get; set; }
    [Export] public RichTextLabel nextDeathText { get; set; }
    
    public async Task Show()
    {
        newDeathText.Text = ("[color=#fa6086]LOOP " + Game.PlayerData.DeathNum + "[/color]");
        nextDeathText.Text = ("[color=#fa6086]LOOP " + (Game.PlayerData.DeathNum + 1) + "[/color]");
        await AnimationPlayer.PlayAsync("full_ui/you_die");
    }
}