namespace Jam;

public interface IUi
{
    EUIState State { get; set; }

    void Init();
}