using Godot;
using GodotAutoOnReady.Attributes;

[GenerateOnReady]
public partial class Main : Node3D
{
    [Export]
    public PackedScene MobScene { get; set; } = null!;

    [GetNode("SpawnPath/SpawnLocation")]
    private PathFollow3D _mobSpawnLocation;

    [GetNode]
    private Player _player;

    [GetNode("Player:Position", NodeType = nameof(Player))]
    private Vector3 _playerInitPos;

    [GetNode]
    private Timer _mobTimer;

    [GetNode("UserInterface/ScoreLabel")]
    private ScoreLabel _scoreLabel;

    [GetNode("UserInterface/Retry")]
    private Control _retry;
    
    [OnReady]
    private void AfterGetNode()
    {
        GD.Print(typeof(Main).Name);
        GD.Print("Player initial pos: ", _playerInitPos);
    }

    [OnReady]
    private void SetupUI()
    {
        _retry.Hide();
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if(@event.IsActionPressed("ui_accept") && _retry.Visible)
        {
            GetTree().ReloadCurrentScene();
        }
    }

    private void OnMobTimerTimeout()
    {
        Mob mob = MobScene.Instantiate<Mob>();
        mob.Squashed += _scoreLabel.OnMobSquashed;
        _mobSpawnLocation.ProgressRatio = GD.Randf();

        mob.Initialize(_mobSpawnLocation.Position, _player.Position);
        AddChild(mob);
    }

    private void OnPlayerHit()
    {
        _mobTimer.Stop();
        _retry.Show();
    }
}
