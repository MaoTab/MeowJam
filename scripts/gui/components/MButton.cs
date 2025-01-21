using Godot;
namespace SnowBlindness;

public partial class MButton : Control
{
    [Export] private bool _style;

    [Export] public Button ButtonBase;
    [Export] public RichTextLabel TextLabel;
    [Export] public bool EnableTexture;
    [Export] private TextureRect _textureRect;
    public override void _Ready()
    {
        TextLabel.Modulate = new Color(.18f,.11f,.20f);
        
        if (!EnableTexture)
        {
            _textureRect.Free();
        }
        
        // 连接鼠标进入和离开事件
        if (ButtonBase != null)
        {
            ButtonBase.Connect("mouse_entered", new Callable(this, nameof(OnMouseEntered)));
            ButtonBase.Connect("mouse_exited", new Callable(this, nameof(OnMouseExited)));
        }
    }

    public void SetText(string text)
    {
        TextLabel.Text = text;
    }
    
    public void SetTexture(Texture2D texture)
    {
        _textureRect.Texture = texture;
    }
    
    private void OnMouseEntered()
    {
        TextLabel.Modulate = new Color(1f,1f,1f);
    }

    private void OnMouseExited()
    {
        TextLabel.Modulate = new Color(.18f,.11f,.20f);
    }
}