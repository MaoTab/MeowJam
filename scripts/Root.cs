using Godot;
using Jam;
using YarnSpinnerGodot;
 
public partial class Root : Node
{
    [Export] private Level _level;
    [Export] private Interface _gui;
    [Export] private Camera _camera;

    public override void _Ready()
    {
        var yarnProject = ResourceLoader.Load<YarnProject>("res://YarnProject.yarnproject");

        Game.Yarn = new YarnRuntime().Init(yarnProject);
        Game.Level = _level.Init();
        Game.Gui = _gui;
        Game.Camera = _camera;
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

        Game.ControlRole.InputMoveDirection = direction;
    }
}