using System.Runtime.InteropServices.ComTypes;
using Godot;

enum AnimationState {
	Idle,
	Moving
}

public partial class Player : CharacterBody2D {
	[Signal] public delegate void PlayerHitEventHandler();
	// due to how simple our game is, we likely do not need to keep track of much state.
	// prioritise writing code quickly over it being pretty; realistically we are very unlikely
	// to reference any of this code in our futures!
	
	const float TOLERANCE = 0.1f;

	// godot doesn't care for readonly's on exposed variables.
	[Export] float SPEED = 2.0f;
	[Export] int POSITION_INCREMENT = 32;

	[ExportGroup("Animation Properties")]
	[Export] AnimatedSprite2D _sprite;
	[Export] SpriteFrames FUTURE_SHEET;
	[Export] SpriteFrames PAST_SHEET;
	bool _isFuture;

	Vector2 _nextMove;

	public override void _Ready() {
		_nextMove         = this.Position;
		_sprite.Animation = "Idle";
		_isFuture         = false;
		_sprite.SetSpriteFrames(PAST_SHEET);
	}

	/*
	// https://docs.godotengine.org/en/4.0/tutorials/inputs/inputevent.html
	// https://docs.godotengine.org/en/stable/tutorials/inputs/input_examples.html
	
	these are all examples for future reference. We're not actually using these!
	
	
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
	
	public override void _UnhandledInput(InputEvent @event) {
		GD.Print(@event.AsText(); // debug
		if (@event is InputEventKey eventKey) {
		 	if (eventKey.pressed && eventKey.Keycode == Key.Escape) {
		 		GetTree().Quit();
			}
		 }
	}
	*/
	
	void GetMovementVector() {
		// As good practice, you should replace UI actions with custom gameplay actions. -Godot
		Vector2 mov   = Input.GetVector("ui_left", "ui_right", "ui_up", "ui_down");
		if (mov.Length() == 0) {
			return;
		}
		
		// 
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
		_sprite.RotationDegrees = angle;
		if (mov.Length() > 0) {
			_nextMove = this.Position + (mov * POSITION_INCREMENT);
		}
		GD.Print(_nextMove);
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
	
	// Use me for input iff you need constant updates.
	public override void _Process(double delta) {
		// get position
		if (this.Position.DistanceTo(_nextMove) < TOLERANCE) {
			GetMovementVector();
			SetAnimationState(AnimationState.Idle);
		} else { // we're moving!
			SetAnimationState(AnimationState.Moving);
		}
		this.Position = this.Position.MoveToward(_nextMove, SPEED * (float)delta);
	}
	
	public void Pause() {
		// implement me
		_sprite.Pause();
	}

	public void Unpause() {
		// implement me
		_sprite.Play();
	}
}