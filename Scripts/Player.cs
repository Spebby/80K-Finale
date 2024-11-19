using System.Runtime.CompilerServices;
using Godot;

enum AnimationState {
	Idle,
	Moving
}

public partial class Player : CharacterBody2D {
	// due to how simple our game is, we likely do not need to keep track of much state.
	// prioritise writing code quickly over it being pretty; realistically we are very unlikely
	// to reference any of this code in our futures!

	[Signal] public delegate void PlayerNotMovingEventHandler();
	[Signal] public delegate void PlayerChangeTimeEventHandler(bool isFuture);
	
	const float TOLERANCE = 0.1f;

	// godot doesn't care for readonly's on exposed variables.
	[Export] float SPEED = 64f;
	[Export] int POSITION_INCREMENT = 32;
	bool moving = false;

	[ExportGroup("Animation Properties")]
	[Export] AnimatedSprite2D _sprite;
	[Export] SpriteFrames FUTURE_SPRITE;
	[Export] SpriteFrames PAST_SPRITE;
	bool _isFuture;

	Vector2 _nextMove;
	RayCast2D _ray;

	Vector2 _lastCheckpoint;
	
	// this is my attempt at recreating the Event Channel pattern that I used commonly in Unity.
	[ExportGroup("Event Channels")]
	[Export] EventChannel OnPlayerDeath;
	
	public override void _Ready() {
		_nextMove			= Position;
		_sprite.Animation = "Idle";
		_isFuture			= false;
		//_sprite.SetSpriteFrames(PAST_SPRITE);
		_ray = GetChild<RayCast2D>(3, true);
		_ray.Enabled = true;
		if (OnPlayerDeath == null) {
			GD.PrintErr($"Player Death Event Channel not configured!");
		}
	}

	/*
	// https://docs.godotengine.org/en/4.0/tutorials/inputs/inputevent.html
	// https://docs.godotengine.org/en/stable/tutorials/inputs/input_examples.html

	// these are all examples for future reference. We're not actually using these!

	// Use me when you need to do an action once
	public override void _Input(InputEvent @event) {
		GD.Print(@event.AsText()); // debug
	}

	/*
	public override void _PhysicsProcess(double delta) {
		if (Input.IsActionPressed("right")) {
			position.X += speed * (float)delta;
		}
	}
	*/

	// tell JIT compiler that this should be inlined
	// helper for exec loop to calculate movement vector
	[MethodImpl(MethodImplOptions.AggressiveInlining)] void UpdateMovementVector() {
		// As good practice, you should replace UI actions with custom gameplay actions. -Godot
		Vector2 mov = Input.GetVector("ui_left", "ui_right", "ui_up", "ui_down");
		if (mov.Length() == 0) 
			return;

		float angle;
		if (Mathf.Abs(mov.X) > Mathf.Abs((mov.Y))) {
			angle = mov.X > 0 ? 90f : 270f;
			mov.X = Mathf.RoundToInt(mov.X);
			mov.Y = 0;
		} else {
			angle = mov.Y > 0 ? 180f : 0F;
			mov.X = 0;
			mov.Y = Mathf.RoundToInt(mov.Y);
		}

		// if we want to use radians, use "SetRotation" function.
		float prevRotation = RotationDegrees;

		// Physics changes are not instant for performance reasons & instead accumulate. B/c Frogger
		// is rotating 90 degrees instantly, we need to update physics immediately.
		if (Mathf.Abs((RotationDegrees = angle) - prevRotation) > Mathf.Epsilon)
			_ray.ForceRaycastUpdate();

		if (_ray.IsColliding()) {
			// if we can get which physics layer is collided w/ that may be helpful, since we should
			// allow players to jump into water, for example.
			return;
		}

		// platform shouldn't affect us while we're moving
		moving = false;
		// will eventually need to revise this code to be more consistent when players jump while moving
		// some frogger games can be pretty inconsistent w/ this so we'll want to make sure we have it down.
		
		// Snap player to next appropriate tile.
		_nextMove = new Vector2(
			Mathf.RoundToInt(Position.X / POSITION_INCREMENT) * POSITION_INCREMENT,
			Mathf.RoundToInt(Position.Y / POSITION_INCREMENT) * POSITION_INCREMENT
		) + (mov * POSITION_INCREMENT);

		// update animation state here b/c won't be called as much
		SetAnimationState(AnimationState.Moving);
	}

	public void EnteredPlatform(MovingObject _platform) {
		//await ToSignal(this, SignalName.PlayerNotMoving);
		platform  =  _platform;
		_platform.Moved += OnPlatformMoved;
	}

	// if make enteredplatform async again, add signal to this function so we don't get as many weird interleavings
	public void ExitPlatform(MovingObject _platform) {
		if (platform == _platform) {
			platform.Moved -= OnPlatformMoved;
			platform       =  null;
		}
	}

	void SetAnimationState(AnimationState state) {
		// ReSharper disable once ArrangeMethodOrOperatorBody
		_sprite.Animation = state switch {
			AnimationState.Idle   => "Idle",
			AnimationState.Moving => "Move",
			var _                 => _sprite.Animation
		};

		_sprite.Play();
	}

	MovingObject platform;
	void OnPlatformMoved(Vector2 mov) {
		if (moving) return;
		GlobalPosition += mov;
		_nextMove      += mov;
	}

	void ShiftTime() {
		//_sprite.SetSpriteFrames(_isFuture ? PAST_SPRITE : FUTURE_SPRITE); // breaks everything atm
		_isFuture = !_isFuture;
		EmitSignal(SignalName.PlayerChangeTime, _isFuture);
	}
	
	// Use me for input iff you need constant updates.
	public override void _PhysicsProcess(double delta) {
		/*	TODO: figure out a better way to trigger animation changes. Investigate signals.
				 *	Mr. Gippity recommends using an animation tree to return to idle when animations are
				 *	done instead of explicitly calling an animation change.
				 */

		// update this check to account for being moved by a platform
		// get position
		if (Position.DistanceTo(_nextMove) < TOLERANCE) {
			moving = false;
			EmitSignal(SignalName.PlayerNotMoving);
			SetAnimationState(AnimationState.Idle);
			UpdateMovementVector();
		}

		if (!moving && Input.IsActionPressed("timeShift")) {
			ShiftTime();
		}

		// this is more snappy, more satisfying but may be too jarring w/ the camera
		Position = Position.Lerp(_nextMove, SPEED * (float)delta * 0.175f);
		//Position = Position.MoveToward(_nextMove, SPEED * (float)delta);
	}

	Marker2D checkpoint;
	public void SetCheckpoint(Marker2D point) => checkpoint = point;
	
	public void Pause() {
		SetPhysicsProcess(false);
		SetProcessUnhandledInput(false);
		SetProcess(false);
		_sprite.Pause();
	}

	public void Unpause() {
		// implement me
		SetProcess(true);
		SetPhysicsProcess(true);
		SetProcessUnhandledInput(true);
		_sprite.Play();
	}

	// KILL THE FROG!
	public void Kill() {
		OnPlayerDeath.TriggerEvent();
		_nextMove      = checkpoint.GlobalPosition;
		GlobalPosition = checkpoint.GlobalPosition;
		moving         = false;
	}
}