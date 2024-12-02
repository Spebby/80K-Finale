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

	Marker2D markerOfInterest;
	List<Marker2D> _anchors;
	Timer exitTimer;
	public override void _Ready() {
		Bounds    ??= GetNodeOrNull<Area2D>("Bounds");
		spriteRef   = GetNode<Sprite2D>("Sprite2D");
		// Sometimes we want platforms that *cant* be stood on, so we'll allow this behaviour.
		if (Bounds != null) {
			Bounds.BodyEntered += StandingOn;
			Bounds.BodyExited  += NotStandingOn;
		}
		
		_anchors = GetChildren().OfType<Marker2D>().ToList();

		ProgressRatio    = 0.001f;
		if (_anchors.Count == 0) {
			markerOfInterest                = new Marker2D();
			markerOfInterest.GlobalPosition = GlobalPosition;
		} else {
			markerOfInterest = _anchors[0];
		}
		prevPos = markerOfInterest.GlobalPosition;

		exitTimer = GetNode<Timer>("Timer");
	}
	
	public void TimeShiftChange(bool isFuture) => spriteRef.SetTexture(isFuture ? FUTURE_SPRITE : PAST_SPRITE);

	float progress = 1f;
	// when player 
	Vector2 prevPos;
	public override void _PhysicsProcess(double delta) {
		ProgressRatio += (float)delta * Speed * progress * 0.175f;
		if (ProgressRatio is >= 1.0f or <= 0) {
			EmitSignal(SignalName.OnProgressComplete);
			if (!Loop) {
				progress = -progress;
			}
		}

		EmitSignal(SignalName.Moved, markerOfInterest.GlobalPosition - prevPos);
		prevPos = markerOfInterest.GlobalPosition;
	}

	void StandingOn(Node2D body) {
		if (body is not Player player) return;
		markerOfInterest = GetClosestAnchor(player.GlobalPosition);
		prevPos          = markerOfInterest.GlobalPosition;
		player.EnteredPlatform(this, markerOfInterest.GlobalPosition);
	}

	Player exitingPlayer;
	void NotStandingOn(Node2D body) {
		if (body is not Player player) return;
		exitingPlayer = player;
		exitTimer.Start();
	}

	void OnExitDelayTimeout() {
		GD.Print("Ooops");
		if (!IsPlayerOnPlatform(exitingPlayer)) {
			exitingPlayer.ExitPlatform(this);
		}
	}
	
	bool IsPlayerOnPlatform(Player player) {
		if (Bounds == null) return false;

		CollisionShape2D collisionShape = Bounds.GetNode<CollisionShape2D>("CollisionShape2D");
		if (collisionShape?.Shape is RectangleShape2D rectShape) {
			// Get the extents of the rectangle shape
			return rectShape.GetRect().HasPoint(player.GlobalPosition);
		}

		return false;
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

	public Marker2D GetClosestAnchor(Vector2 pos) {
		if (_anchors.Count == 0 || Bounds == null) {
			GD.PrintErr($"No anchors defined for {Name}. Falling back on global position.");
			return markerOfInterest;
		}
		
		Marker2D closest                = _anchors[0];
		float   closestDistanceSquared = pos.DistanceSquaredTo(closest.GlobalPosition);

		foreach (var anchor in _anchors) {
			float distanceSquared = pos.DistanceSquaredTo(anchor.GlobalPosition);
			if (distanceSquared < closestDistanceSquared) {
				closest                = anchor;
				closestDistanceSquared = distanceSquared;
			}
		}

		return closest;
	}
	
	public Area2D GetCollider() {
		return Bounds;
	}
}