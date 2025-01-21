namespace SnowBlindness;

public interface IUi
{
    EUIState State { get; set; }

    void Init();
}