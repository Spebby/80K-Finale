using Godot;

public partial class MovingObject : PathFollow2D, ITimeShiftable, IPauseable {
	[ExportGroup("Object Settings")]
	const float TOLERANCE = 0.1f;
	[Export] public float Speed = 32f;
	[Export] protected Area2D Bounds;

	[ExportGroup("Texture Settings")]
	[Export] CompressedTexture2D PAST_SPRITE;
	[Export] CompressedTexture2D FUTURE_SPRITE;
	Sprite2D spriteRef;

	[Signal] public delegate void MovedEventHandler(Vector2 mov);
	[Signal] public delegate void OnProgressCompleteEventHandler();

	bool canBeStoodOn;

	public override void _Ready() {
		Bounds    ??= GetNode<Area2D>("Bounds");
		spriteRef   = GetNode<Sprite2D>("Sprite2D");
		// Sometimes we want platforms that *cant* be stood on, so we'll allow this behaviour.
		if (Bounds != null) {
			Bounds.BodyEntered += StandingOn;
			Bounds.BodyExited  += NotStandingOn;
		}

		prevPos = Position;
	}
	
	public void TimeShiftChange(bool isFuture) => spriteRef.SetTexture(isFuture ? FUTURE_SPRITE : PAST_SPRITE);

	float progress = 1f;
	Vector2 prevPos;
	public override void _PhysicsProcess(double delta) {
		ProgressRatio += (float)delta * Speed * progress * 0.175f;
		if (ProgressRatio is >= 1.0f or <= 0f) {
			EmitSignal(SignalName.OnProgressComplete);
			if (!Loop) {
				progress = -progress;
			}
		}

		EmitSignal(SignalName.Moved, GlobalPosition - prevPos);
		prevPos = GlobalPosition;
	}

	void StandingOn(Node2D body) {
		if (body is not Player player) return;
		player.EnteredPlatform(this);
	}

	void NotStandingOn(Node2D body) {
		if (body is not Player player) return;
		player.ExitPlatform(this);
	}

	public void Pause() {
		SetPhysicsProcess(false);
		SetProcessUnhandledInput(false);
		SetProcess(false);
	}

	public void Unpause() {
		SetProcess(true);
		SetPhysicsProcess(true);
		SetProcessUnhandledInput(true);
	}
	
	public Area2D GetCollider() {
		return Bounds;
	}
}