using System.Collections.Generic;
using System.Linq;
using Godot;

public partial class MovingObject : PathFollow2D, IPlatform, ITimeShiftable, IPauseable {
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

	List<Marker2D> _anchors;
	public override void _Ready() {
		Bounds    ??= GetNode<Area2D>("Bounds");
		spriteRef   = GetNode<Sprite2D>("Sprite2D");
		// Sometimes we want platforms that *cant* be stood on, so we'll allow this behaviour.
		if (Bounds != null) {
			Bounds.BodyEntered += StandingOn;
			Bounds.BodyExited  += NotStandingOn;
		}
		
		_anchors = GetChildren().OfType<Marker2D>().ToList();

		ProgressRatio = 0.001f;
		prevPos       = Position;
	}
	
	public void TimeShiftChange(bool isFuture) => spriteRef.SetTexture(isFuture ? FUTURE_SPRITE : PAST_SPRITE);

	float progress = 1f;
	Vector2 prevPos;
	public override void _PhysicsProcess(double delta) {
		ProgressRatio += (float)delta * Speed * progress * 0.175f;
		if (ProgressRatio is >= 1.0f or <= 0) {
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

	public Vector2 GetClosestAnchor(Vector2 pos) {
		return GlobalPosition;
		// Everything below works but it's too unstable right now.
		
		if (_anchors.Count == 0) {
			GD.PrintErr($"No anchors defined for {Name}. Falling back on global position.");
			return GlobalPosition;
		}
		Vector2 closest                = _anchors[0].GlobalPosition;
		float   closestDistanceSquared = pos.DistanceSquaredTo(closest);

		foreach (var anchor in _anchors) {
			float distanceSquared = pos.DistanceSquaredTo(anchor.GlobalPosition);
			if (distanceSquared < closestDistanceSquared) {
				closest                = anchor.GlobalPosition;
				closestDistanceSquared = distanceSquared;
			}
		}

		return closest;
	}
	
	public Area2D GetCollider() {
		return Bounds;
	}
}