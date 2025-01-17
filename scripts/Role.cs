using System;
using System.Collections.Generic;
using Godot;

namespace Jam;

public partial class Role : CharacterBody2D , ISelect
{
    /// <summary>
    /// 移动模式
    /// </summary>
    public EMoveMode MoveMode = EMoveMode.Input;
    
    [Export] public float MoveSpeed = 50f;
    
    /// <summary>
    /// 需要移动到的位置
    /// </summary>
    public Vector2 MovePos;
    
    /// <summary>
    /// 是否允许移动
    /// </summary>
    public bool Movable = true;
    
    /// <summary>
    /// 当移动到终点时
    /// </summary>
    public Action OnMoveComplete;
    
    public enum EMoveMode
    {
        Input,
        Path,
        Point
    }
    
    /// <summary>
    /// 输入方向
    /// </summary>
    public Vector2 InputMoveDirection;
    
    /// <summary>
    /// 鼠标碰撞盒
    /// </summary>
    [Export] public Area2D MouseCollider;
    
    /// <summary>
    /// 鼠标碰撞盒形状
    /// </summary>
    public CollisionShape2D MouseColliderShape;
    
    /// <summary>
    /// 绘制节点，用于存放所有精灵渲染器的父节点
    /// </summary>
    [Export] public Node2D DrawNode;
   
    /// <summary>
    /// 身体部位
    /// </summary>
    public Godot.Collections.Dictionary<string, AnimatedSprite2D> BodyParts = new();
    
    /// <summary>
    /// 接受对话的距离
    /// </summary>
    [Export] public float TalkDistance = 20f;
    
    public Vector2 CameraFollowPos;
    
    /// <summary>
    /// 角色朝向
    /// </summary>
    public EFaceDir FaceDir = EFaceDir.E;
    
    /// <summary>
    /// 跑步
    /// </summary>
    public bool Run;
    
    public enum EFaceDir
    {
        /// <summary>
        /// 左
        /// </summary>
        E,
        /// <summary>
        /// 右
        /// </summary>
        W
    }

    public bool StopPhysicsProcess;
    private ShaderMaterial _shader;

    /// <summary>
    /// 距离容差
    /// </summary>
    public float DistanceTolerance = 1f;
    
    /// <summary>
    /// 当前动画状态
    /// </summary>
    public AnimateState CurAnimateState = AnimateState.Idle;
    public enum AnimateState
    {
        Idle,
        Walk,
        Run
    }
    
    // MARK: - Init()
    public Role Init()
    {
        // 初始化移动位置
        MovePos = Position;
        
        _shader = (ShaderMaterial)Material;
        
        if (MouseCollider.GetChild(0) is CollisionShape2D mouseColliderShape)
        {
            MouseColliderShape = mouseColliderShape;
            MouseCollider.MouseEntered += MouseEntered;
            MouseCollider.MouseExited += MouseExited;
        }
        
        foreach (var node in DrawNode.GetChildren())
        {
            if (node is AnimatedSprite2D animatedSprite2D)
            {
                BodyParts.Add(node.Name, animatedSprite2D);
            }
        }

        UpdateFaceDir();
        PlayAnimate(AnimateState.Idle, true);
        return this;
    }
    
    // MARK: - PhysicsProcess()
    public void PhysicsProcess()
    {
        if(StopPhysicsProcess) return;
        
        CameraFollowPos = Position;
        
        Move();
        Animate();
    }
    
    
    // MARK: - MouseExited()
    private new void MouseExited()
    {
        if(!Game.CanControl) return;
        
        if (Game.Select == this)
        {
            Game.Select = null;
        }
        else
        {
            return;
        }

        DisableHover();
    }
    
    // MARK: - MouseEntered()
    private new void MouseEntered()
    {
        if(!Game.CanControl) return;
        if (Game.Select == null)
        {
            Game.Gui.RemoveAllTip();
            Game.Select = this;
        }
        else
        {
            if (Game.Select == this)
            {
                Game.Gui.RemoveAllTip();
            }
            else
            {
                return;
            }
        }
        
        List<string> tipList;

        if (Game.ControlRole == this) // 玩家选中自身时
        {
            tipList = new List<string>();

            tipList.Add("雪莉");
            tipList.Add("line");
            tipList.Add("[i][color=#c17a56]遗失了记忆的猫，曾经是个人类。[/color][/i]");
            tipList.Add("[i][color=#c17a56]虽然不擅长为人处世，但却意外的热心肠，是个可靠的帮手。[/color][/i]");
            tipList.Add("[i][color=#c17a56]不过，总觉得她的身上还隐藏着更大的秘密……[/color][/i]");
            tipList.Add("[i][color=#c17a56]撕哈，撕哈---[/color][/i]");
        }
        else
        {
            tipList = new List<string>();
            tipList.Add("这是ta。");
            tipList.Add("line");

            if (IsTalkable())
            {
                tipList.Add("左键对话。");
            }
            else
            {
                tipList.Add("你得再靠近点才能跟他交互。");
            }

            tipList.Add("line");
            tipList.Add("[i][color=#c17a56]也没什么特别的。[/color][/i]");
        }

        Game.Gui.CreatListTip(tipList);

        GameEvent.OnMouseLeftDown += OnClick;

        _shader.SetShaderParameter("thickness", 1f);
    }

    public void DisableHover()
    {
        Game.Gui.RemoveAllTip();
        GameEvent.OnMouseLeftDown -= OnClick;
        _shader.SetShaderParameter("thickness", 0f);
    }
    
    // MARK: - OnClick()
    public void OnClick(Vector2 _)
    {
        if(!Game.CanControl) return;
        if (IsTalkable() && Game.ControlRole != this)
        {
            DisableHover();
            
            // 对话时调整自身的朝向，避免背着身子跟对方对话
            Face2Char(Game.ControlRole);
            Game.ControlRole.Face2Char(this);
            Game.CanControl = false;
            Game.Yarn.PlayNode("Node_对话");
        }
    }
    
    // MARK: - PlayAnimate()
    public void PlayAnimate(AnimateState animateState, bool refresh = false)
    {
        if (!refresh && CurAnimateState == animateState) return;

        CurAnimateState = animateState;

        foreach (var part in BodyParts.Values)
        {
            part.Stop();
            part.Frame = 0;

            switch (animateState)
            {
                case AnimateState.Idle:
                    // part.SetSpeedScale(1);
                    part.Play("Idle");
                    break;
                case AnimateState.Walk:
                    // part.SetSpeedScale(MoveSpeed / 25f);
                    part.Play("Walk");
                    break;
                case AnimateState.Run:
                    part.Play("Run");
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(animateState), animateState, null);
            }
        }
    }
    
    // MARK: - Face2Char()
    /// <summary>
    /// 将当前角色朝向目标角色
    /// </summary>
    /// <param name="role">目标角色</param>
    public void Face2Char(Role role)
    {
        if (role.Position.X < Position.X)
        {
            FaceDir = EFaceDir.W;
        }
        else
        {
            FaceDir = EFaceDir.E;
        }
        
        UpdateFaceDir();
    }
    
    // MARK: - MovePath()
    public void MovePath(Action onFinished)
    {
        if (MoveMode == EMoveMode.Path)
        {
            onFinished?.Invoke();
            return;
        }

        MoveMode = EMoveMode.Path;
        
        var paths = new List<Vector2>();
        
        // destination.Modulate = new Color(1, 0, 0, 1);
    }
    
    // MARK: - MoveTo()
    public void MoveTo(Vector2 pos,Action onComplete = null)
    {
        MoveMode = EMoveMode.Point;
        MovePos =  pos;
        OnMoveComplete = onComplete;
    }

    
    // MARK: - Move()
    public void Move()
    {
        switch (MoveMode)
        {
            case EMoveMode.Input:
                break;
            case EMoveMode.Path:
                return; 
            case EMoveMode.Point:
                // 非输入性移动
                if (Position.DistanceTo(MovePos) <= DistanceTolerance)
                {
                    if (OnMoveComplete != null)
                    {
                        OnMoveComplete.Invoke();
                        OnMoveComplete = null;
                        return;
                    }
                }
                Position = MathfHelper.V2Lerp(Position, MovePos, MoveSpeed * .5f /*点对点移动硬核降速*/ * (float)Game.PhysicsDelta);
                return; 
            default:
                throw new ArgumentOutOfRangeException();
        }
        
        MovePos = Position + InputMoveDirection;
        
        if(!Movable)
        {
            return;
        }
        
        var inputMoveDirection = InputMoveDirection.Normalized();
        
        // 位移部分
        var collision =
            Run ? MoveAndCollide(inputMoveDirection * MoveSpeed * 3 * (float)Game.PhysicsDelta) 
                : MoveAndCollide(inputMoveDirection * MoveSpeed * (float)Game.PhysicsDelta);
        
        
        // if (collision != null)
        // {
        //     Velocity = Velocity.Slide(collision.GetNormal());
        // }

        MoveAndSlide();
    }
    
    // MARK: - Animate()
    public void Animate()
    {
        if (!MathfHelper.AetF(Position.X,MovePos.X))
        {
            // 根据 direction.X 的值设置 FlipH
            bool flip = Position.X > MovePos.X; // 判断是否需要翻转
            foreach (var part in BodyParts.Values)
            {
                part.FlipH = flip; // 统一设置 FlipH
            }
        }
        
        if (Position.DistanceTo(MovePos) <= DistanceTolerance)
        {
            PlayAnimate(AnimateState.Idle);
        }
        else
        {
            if (Run)
            {
                PlayAnimate(AnimateState.Run);
            }
            else
            {
                PlayAnimate(AnimateState.Walk);
            }
        }
        
        // 根据距离调整动画速度
        var speedScale= float.Clamp(Position.DistanceTo(MovePos) * 2, 0f, 1f);
        foreach (var part in BodyParts.Values)
        {
            part.SpeedScale = speedScale;
        }
    }
    
    // MARK: - IsTalkable()
    /// <summary>
    /// 在进行对话前的检测，进入自身的范围内时允许对话
    /// </summary>
    /// <returns></returns>
    public bool IsTalkable()
    {
        // 与玩家距离小于XX则可以对话
        return Game.ControlRole.GetDistanceTo(Position) <= Game.ControlRole.TalkDistance + TalkDistance;
    }
    
    // MARK: - GetDistanceTo()
    /// <summary>
    /// 计算距离
    /// </summary>
    /// <param name="pos"></param>
    /// <returns></returns>
    public float GetDistanceTo(Vector2 pos)
    {
        return Position.DistanceTo(pos);
    }
    
    // MARK: - GetGlobalRect()
    private Rect2 GetGlobalRect()
    {
        // 获取 Area2D 的形状并计算全局矩形
        var shape = MouseColliderShape;
        if (shape != null && shape.Shape is RectangleShape2D rectangleShape)
        {
            // 获取形状的矩形
            Vector2 position = MouseColliderShape.GlobalPosition; // 获取全局位置
            Vector2 size = rectangleShape.Size; // 获取大小
            return new Rect2(position - rectangleShape.Size, size); // 创建全局矩形
        }

        return new Rect2(); // 返回空矩形
    }
    
    public void UpdateFaceDir()
    {
        foreach (var part in BodyParts)
        {
            switch (FaceDir)
            {
                case EFaceDir.E:
                    part.Value.FlipH = false;
                    break;
                case EFaceDir.W:
                    part.Value.FlipH = true;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }

    public void Input(Vector2 direction,bool isRun)
    {
        if(!Game.CanControl) return;
        if (isRun)
        {
            InputMoveDirection = direction * 3;
            Run = true;
        }
        else
        {
            InputMoveDirection = direction;
            Run = false;
        }
    }
}