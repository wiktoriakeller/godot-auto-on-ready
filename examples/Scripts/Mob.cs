using Godot;

public partial class Mob : CharacterBody3D
{
    [Signal]
    public delegate void SquashedEventHandler();

    [Export]
    public int MinSpeed { get; set; } = 10;

    [Export] 
    public int MaxSpeed { get; set; } = 18;

    public override void _PhysicsProcess(double delta)
    {
        MoveAndSlide();
    }

    public void Initialize(Vector3 startPosition, Vector3 playerPosition)
    {
        LookAtFromPosition(startPosition, playerPosition, Vector3.Up);
        RotateY((float)GD.RandRange(-Mathf.Pi / 4.0d, Mathf.Pi / 4.0d));

        var randomSpeed = GD.RandRange(MinSpeed, MaxSpeed);
        Velocity = Vector3.Forward * randomSpeed;
        Velocity = Velocity.Rotated(Vector3.Up, Rotation.Y);
    }

    public void Squash()
    {
        EmitSignal(SignalName.Squashed);
        QueueFree();
    }

    private void OnVisibilityNotifierScreenExited()
    {
        QueueFree();
    }
}
