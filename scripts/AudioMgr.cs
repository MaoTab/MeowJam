


using System.Threading.Tasks;
using Godot;
using SnowBlindness.scripts;

public partial class AudioMgr : Node
{
    [Export] private AudioPlayer MainAudioPlayer;
    [Export] private AudioPlayer LoopAudioPlayer;
    
    [Export] private AudioPlayer DengShanAudioPlayer;
    [Export] private AudioPlayer DengShanLoopAudioPlayer;
    
    public async Task Init()
    {

        MainAudioPlayer.Finished += () =>
        {
            LoopAudioPlayer.Play();
        };
        
        MainAudioPlayer.Play(0.8f);
    }

    public void PlayDengShan()
    {
        LoopAudioPlayer.Stop();
        DengShanAudioPlayer.Finished += PlayDengShanAudio;
        DengShanAudioPlayer.Play();
    }

    private void PlayDengShanAudio()
    {
        DengShanLoopAudioPlayer.Play();
        DengShanAudioPlayer.Finished -= PlayDengShanAudio;
    }
    
    public void StopDengShan()
    {
        DengShanAudioPlayer.Stop();
        DengShanLoopAudioPlayer.Stop();
        
    }
}