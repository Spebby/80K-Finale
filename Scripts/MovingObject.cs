using Godot;

public partial class MovingObject : PathFollow2D {
	const float TOLERANCE = 0.1f;
	[Export] protected float Speed = 32f;
	[Export] protected Area2D Bounds;

	[Signal] public delegate void MovedEventHandler(Vector2 mov);

	public override void _Ready() {
		Bounds             ??= GetNode<Area2D>("Bounds");
		Bounds.BodyEntered +=  StandingOn;
		prevPos            =   Position;
	}

	float progress = 1f;
	Vector2 prevPos;
	public override void _PhysicsProcess(double delta) {
		ProgressRatio += (float)delta * Speed * progress * 0.175f;
		if (!Loop && ProgressRatio is >= 1.0f or <= 0f) {
			progress = -progress;
		}

		EmitSignal(SignalName.Moved, Position - prevPos);
		prevPos = Position;
	}

	async void StandingOn(Node2D body) {
		if (body is not Player player) return;
		await ToSignal(player, Player.SignalName.PlayerNotMoving);
		GD.Print("Not moving!");
		player.EnteredPlatform(this);
	}
}