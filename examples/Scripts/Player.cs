using Godot;
using GodotAutoOnReady.Attributes;

[GenerateOnReady]
public partial class Player : CharacterBody3D
{
    [Signal]
    public delegate void HitEventHandler();

    [Export]
    public int Speed { get; set; } = 14;
    
    [Export]
    public int FallAcceleration { get; set; } = 75;

    [Export]
    public int JumpImpulse { get; set; } = 20;

    [Export]
    public int BounceImpulse { get; set; } = 16;

    [GetNode]
    private Node3D _pivot;

    private Vector3 _targetVelocity = Vector3.Zero;

    public override void _PhysicsProcess(double delta)
    {
        var direction = Vector3.Zero;

        //Ground velocity
        if(Input.IsActionPressed("move_right"))
        {
            direction.X += 1.0f;
        }

        if(Input.IsActionPressed("move_left"))
        {
            direction.X -= 1.0f;
        }

        if (Input.IsActionPressed("move_back"))
        {
            direction.Z += 1.0f;
        }

        if (Input.IsActionPressed("move_forward"))
        {
            direction.Z -= 1.0f;
        }

        if(direction != Vector3.Zero)
        {
            direction = direction.Normalized();
            _pivot.Basis = Basis.LookingAt(direction);
        }

        _targetVelocity.X = direction.X * Speed;
        _targetVelocity.Z = direction.Z * Speed;

        //Vertical velocity
        if(!IsOnFloor())
        {
            _targetVelocity.Y -= FallAcceleration * (float)delta;
        }

        if (IsOnFloor() && Input.IsActionJustPressed("jump"))
        {
            _targetVelocity.Y = JumpImpulse;
        }

        //All collisions in this frame
        for (int i = 0; i < GetSlideCollisionCount(); i++)
        {
            KinematicCollision3D collision = GetSlideCollision(i);

            if (collision.GetCollider() is Mob mob)
            {
                //Hitting mob from above
                if (Vector3.Up.Dot(collision.GetNormal()) > 0.1f)
                {
                    mob.Squash();
                }
            }
        }

        Velocity = _targetVelocity;
        MoveAndSlide();
    }

    private void Die()
    {
        EmitSignal(SignalName.Hit);
        QueueFree();
    }

    private void OnMobDetectorBodyEntered(Node3D body)
    {
        Die();
    }
}
