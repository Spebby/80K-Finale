using System;
using System.Runtime.CompilerServices;
using Godot;

enum AnimationState {
	Idle,
	Moving
}

enum CollisionLayers : uint {
	Wall = 1 << 0,
	MovingPlatform = 1 << 1,
	Water = 1 << 2,
	Hazard = 1 << 3,
	Room = 1 << 4
}

public partial class Player : CharacterBody2D, IPauseable {
	// due to how simple our game is, we likely do not need to keep track of much state.
	// prioritise writing code quickly over it being pretty; realistically we are very unlikely
	// to reference any of this code in our futures!
	const float TOLERANCE = 0.1f;

	[ExportGroup("Movement & Collision")]
	// godot doesn't care for readonly's on exposed variables.
	[Export] float SPEED = 64f;
	[Export] int POSITION_INCREMENT = 32;
	bool moving;
	bool markedForDeath;

	bool ShouldIDie => !moving && markedForDeath && (_platform == null);
	
	[Export(PropertyHint.Layers2DPhysics)] uint _movingCollisionLayers { get; set; } = 0;

	[ExportGroup("Animation Properties")]
	[Export] AnimatedSprite2D _sprite;
	[Export] SpriteFrames FUTURE_SPRITE;
	[Export] SpriteFrames PAST_SPRITE;
	bool _isFuture;

	Vector2 _nextMove;
	RayCast2D _ray;

	uint _orgCollisionLayer;
	Vector2 _lastCheckpoint;

	// There is definitely a better way of doing this, but working > proper when there's a deadline.
	[ExportGroup("Sounds")]
	[Export] AudioStreamPlayer2D walkSFX;
	[Export] AudioStream groundSFX;
	[Export] AudioStream platformSFX;
	[Export] AudioStream timeShiftSFX;
	[Export] AudioStream declineSFX;

	[ExportGroup("Time Shift")]
	[Export] double timeShiftCooldown = 8f;
	double _cooldown = 0;
	
	// this is my attempt at recreating the Event Channel pattern that I used commonly in Unity.
	[ExportGroup("Event Channels")]
	[Export] EventChannel OnPlayerDeath;
	[Export] BoolEventChannel OnTimeShift;
	
	public override void _Ready() {
		_nextMove         = GlobalPosition;
		_sprite.Animation = "Idle";
		_isFuture         = false;
		_cooldown         = timeShiftCooldown;

		//_sprite.SetSpriteFrames(PAST_SPRITE);
		_ray         = GetNode<RayCast2D>("RayCast2D");
		_ray.Enabled = true;
		if (OnPlayerDeath == null) {
			throw new Exception($"{Name} -- OnPlayerDeath Event Channel not configured!");
		}

		if (OnTimeShift == null) {
			throw new Exception($"{Name} -- OnTimeChange Event Channel missing. Assign now!");
		}

		_orgCollisionLayer = CollisionLayer;
	}

	/*
	// https://docs.godotengine.org/en/4.0/tutorials/inputs/inputevent.html
	// https://docs.godotengine.org/en/stable/tutorials/inputs/input_examples.html

	// these are all examples for future reference. We're not actually using these!
	*/

	// tell JIT compiler that this should be inlined
	// helper for exec loop to calculate movement vector
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	bool UpdateMovementVector() {
		// As good practice, you should replace UI actions with custom gameplay actions. -Godot
		Vector2 mov = Input.GetVector("ui_left", "ui_right", "ui_up", "ui_down");
		if (mov.Length() == 0 || moving) {
			return false;
		}

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

		GodotObject hit = _ray.GetCollider();
		// this is terrible but I don't care. I'll learn real collision detection later.
		if (hit != null) {
			if (hit is TileMapLayer target) {

				// We don't want to kill player instantly if they're jumping onto water, so we "mark them"
				// for death, and will kill them in Physics Process if they're not moving and still marked.

				if (target.HasMeta("KillPlayer")) {
					markedForDeath = true;
				} else {
					return false;
				}
			} else {
				markedForDeath = false;
				return false;
			}
		} else {
			markedForDeath = false;
		}

		// platform shouldn't affect us while we're moving
		moving = true;
		SetAnimationState(AnimationState.Moving);
		SetCollision(false);
		PlayStepAudio(groundSFX);
		// will eventually need to revise this code to be more consistent when players jump while moving
		// some frogger games can be pretty inconsistent w/ this so we'll want to make sure we have it down.
		
		// Snap player to next appropriate tile.
		_nextMove = new Vector2(
			Mathf.RoundToInt(GlobalPosition.X / POSITION_INCREMENT) * POSITION_INCREMENT,
			Mathf.RoundToInt(GlobalPosition.Y / POSITION_INCREMENT) * POSITION_INCREMENT
		) + (mov * POSITION_INCREMENT);

		// update animation state here b/c won't be called as much
		return true;
	}

	IPlatform _platform;
	public void EnteredPlatform(IPlatform platform) {
		//await ToSignal(this, SignalName.PlayerNotMoving);
		if (_platform != null) return;
		PlayStepAudio(platformSFX);
		_platform =  platform;
		if (platform is MovingObject obj) {
			obj.Moved += OnPlatformMoved;
		}
	}

	// if make enteredplatform async again, add signal to this function so we don't get as many weird interleavings
	public void ExitPlatform(IPlatform platform) {
		if (_platform == platform) {
			if (_platform is MovingObject obj) {
				obj.Moved -= OnPlatformMoved;
			}

			_platform       =  null;
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

	void OnPlatformMoved(Vector2 mov) {
		if (moving) return;
		GlobalPosition += mov;
		_nextMove      += mov;
	}
	
	void ShiftTime() {
		GD.Print(_cooldown);
		if (_cooldown > 0) {
			PlayStepAudio(declineSFX, 0.6f, 1.1f);
			return;
		}
		
		_isFuture = !_isFuture;
		_sprite.SpriteFrames = _isFuture ? PAST_SPRITE : FUTURE_SPRITE;
		OnTimeShift.TriggerEvent(_isFuture);
		PlayStepAudio(timeShiftSFX, 1, 1);
		_cooldown = timeShiftCooldown;
	}
	
	public override void _Input(InputEvent @event) {
		if (!moving && @event.IsActionPressed("timeShift")) {
			//GD.Print("Action 'ui_accept' triggered via specific input: " + @event);
			ShiftTime();
		}
	}

	// Use me for input iff you need constant updates.
	public override void _PhysicsProcess(double delta) {
		/*	TODO: figure out a better way to trigger animation changes. Investigate signals.
		 *	recommended to use an animation tree to return to idle when animations are
		 *	done instead of explicitly calling an animation change.
		 */

		// TODO: kill player if they're inside now hostile terrain
		/*
		if (recheckCollision) {
			recheckCollision = false;
			if (_ray.IsColliding()) {
				GD.Print("Inside something");
				Kill();
			}
		}*/
		
		_cooldown -= delta;
		// update this check to account for being moved by a platform
		// get position
		if (GlobalPosition.DistanceTo(_nextMove) < TOLERANCE) {
			if (ShouldIDie) {
				Kill();
				return;
			}
			
			SetAnimationState(AnimationState.Idle);
			// we reset moving after, to prevent player from moving before we can do collision checks.
			
			if (!UpdateMovementVector()) {
				SetCollision(true);
				moving = false;
			}
		}

		GlobalPosition = GlobalPosition.Lerp(_nextMove, SPEED * (float)delta * 0.175f);
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
		SetProcess(true);
		SetPhysicsProcess(true);
		SetProcessUnhandledInput(true);
		_sprite.Play();
	}

	// KILL THE FROG!
	public void Kill() {
		GD.Print("I should be dead right now!");
		markedForDeath = false;
		OnPlayerDeath.TriggerEvent();
		_nextMove      = checkpoint.GlobalPosition;
		GlobalPosition = checkpoint.GlobalPosition;
		moving         = false;
		SetCollision(true);
	}

	RandomNumberGenerator random = new RandomNumberGenerator();
	void PlayStepAudio(AudioStream sample, float minPitch = 0.8f, float maxPitch = 1.2f) {
		random.Randomize();
		walkSFX.PitchScale = random.RandfRange(minPitch, maxPitch);
		walkSFX.Stream     = sample;
		walkSFX.Play();
	}
	
	void SetCollision(bool val) {
		CollisionLayer = val ? _orgCollisionLayer : _movingCollisionLayers;
	}
}