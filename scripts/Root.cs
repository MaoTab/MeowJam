using Godot;
using Jam;
using Jam.scripts;
using YarnSpinnerGodot;
using Interface = Jam.scripts.Interface;

public partial class Root : Node
{
    [Export] private Level _level;
    [Export] private Interface _gui;
    [Export] private Camera _camera;

    [Export] private Button startButton;
    [Export] private AnimationPlayerPlus startAnimationPlayer;
    [Export] private AudioMgr audio;
    [Export] private Control UIROOT;
    
    public override void _Ready() 
    {
        var yarnProject = ResourceLoader.Load<YarnProject>("res://YarnProject.yarnproject");
 
        Game.Yarn = new YarnRuntime().Init(yarnProject);
        Game.Yarn.Start();
        Game.Level = _level.Init();
        Game.PlayerData = new PlayerData().Init();
        Game.Gui = _gui;
        Game.Gui.Init();
        Game.Camera = _camera;
        Game.CanControl = false;
        Game.Audio = audio;
        audio.Init();
        
        startButton.ButtonUp += async () =>
        {
            await startAnimationPlayer.PlayAsync("main/Start");
            Game.Yarn.PlayNode("Node_对话");
        };
    }

    public override void _Process(double delta)
    {
        Game.Gui.Process();
    }

    public override void _PhysicsProcess(double delta)
    {
        Game.PhysicsDelta = delta;
        Game.SceneMousePos = _level.GetGlobalMousePosition(); // 获取鼠标在场景中的位置
        Game.Camera.PhysicsProcess();
        Game.Level.PhysicsProcess();
        Game.Gui.PhysicsProcess();
    }

    public override void _Input(InputEvent @event)
    {
        Game.MousePos = GetViewport().GetMousePosition();

        // 键鼠控制
        // 检查事件是否是鼠标按钮事件
        if (@event is InputEventMouseButton mouseButtonEvent)
        {
            // 检查是否是左键按下事件
            if (mouseButtonEvent.ButtonIndex == MouseButton.Left && mouseButtonEvent.IsPressed())
            {
                if (GameEvent.OnMouseLeftDown != null)
                {
                    GameEvent.OnMouseLeftDown.Invoke(Game.MousePos);

                    if (GameEvent.OnMouseLeftDownInScene != null)
                    {
                        GameEvent.OnMouseLeftDownInScene.Invoke(Game.MousePos);
                    }
                }
            }
        }
        
        var direction = new Vector2();
        // 检查 WASD 键的输入
        if (Input.IsActionPressed("w"))
        {
            direction.Y -= 1; // 向上
        }

        if (Input.IsActionPressed("s"))
        {
            direction.Y += 1; // 向下
        }

        if (Input.IsActionPressed("a"))
        {
            direction.X -= 1; // 向左
        }

        if (Input.IsActionPressed("d"))
        {
            direction.X += 1; // 向右
        }

        if (Input.IsActionJustReleased("ScrollUp"))
        {
            GameEvent.OnMouseScrollWheelUp?.Invoke();
        }
        
        if (Input.IsActionPressed("Esc"))
        {
            GetTree().Quit();
        }
        
        Game.ControlRole.Input(direction, Input.IsActionPressed("shift"));
    }
}