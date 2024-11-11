using Godot;

public partial class Player : CharacterBody2D {
	[Signal] public delegate void playerHitEventHandler();
	// due to how simple our game is, we likely do not need to keep track of much state.
	// prioritise writing code quickly over it being pretty; realistically we are very unlikely
	// to reference any of this code in our futures!

	[Export] Sprite2D _sprite;
	
	[Export] public float Speed = 300.0f;
	public override void _PhysicsProcess(double delta)
	{
		Vector2 velocity = Velocity;

		// Get the input direction and handle the movement/deceleration.
		// As good practice, you should replace UI actions with custom gameplay actions.
		Vector2 direction = Input.GetVector("ui_left", "ui_right", "ui_up", "ui_down");
		if (direction != Vector2.Zero)
		{
			velocity.X = direction.X * Speed;
		}
		else
		{
			velocity.X = Mathf.MoveToward(Velocity.X, 0, Speed);
		}

		Velocity = velocity;
		MoveAndSlide();
	}
	
	public override void _Ready() {
		
	}
	
	public void Pause() {
		// implement me
	}

	public void Unpause() {
		// implement me
	}
}