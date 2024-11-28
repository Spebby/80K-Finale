using Godot;

public partial class Platform : Area2D, IPlatform, ITimeShiftable {
	[Export] CompressedTexture2D PAST_SPRITE;
	[Export] CompressedTexture2D FUTURE_SPRITE;
	[Export] bool staticSprite;
	Sprite2D spriteRef;
	
	public override void _Ready() {
		spriteRef   = GetNode<Sprite2D>("Sprite2D");
		// Sometimes we want platforms that *cant* be stood on, so we'll allow this behaviour.
		BodyEntered += StandingOn;
		BodyExited  += NotStandingOn;
	}

	public void TimeShiftChange(bool isFuture) {
		if (staticSprite) return;
		spriteRef.SetTexture(isFuture ? FUTURE_SPRITE : PAST_SPRITE);
	}

	void StandingOn(Node2D body) {
		if (body is not Player player) return;
		player.EnteredPlatform(this);
	}

	void NotStandingOn(Node2D body) {
		if (body is not Player player) return;
		player.ExitPlatform(this);
	}

	public Vector2 GetClosestAnchor(Vector2 pos) {
		return GlobalPosition;
	}
}