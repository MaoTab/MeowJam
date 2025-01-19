


using System.Threading.Tasks;
using Godot;
using Jam.scripts;

public partial class AudioMgr : Node
{
    [Export] private AudioPlayer MainAudioPlayer;
    [Export] private AudioPlayer LoopAudioPlayer;
    
    
    public async Task Init()
    {

        MainAudioPlayer.Finished += () =>
        {
            LoopAudioPlayer.Play();
        };
        
        MainAudioPlayer.Play(0.8f);
    }
}