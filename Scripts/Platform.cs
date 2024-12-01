using System.Collections.Generic;
using System.Linq;
using Godot;

public partial class Platform : Area2D, IPlatform, ITimeShiftable {
	[Export] CompressedTexture2D PAST_SPRITE;
	[Export] CompressedTexture2D FUTURE_SPRITE;
	[Export] bool staticSprite;
	Sprite2D spriteRef;

	Marker2D fallbackMarker;
	List<Marker2D> _anchors;
	public override void _Ready() {
		spriteRef   = GetNode<Sprite2D>("Sprite2D");
		// Sometimes we want platforms that *cant* be stood on, so we'll allow this behaviour.
		BodyEntered += StandingOn;
		BodyExited  += NotStandingOn;
		
		_anchors = GetChildren().OfType<Marker2D>().ToList();
		if (_anchors.Count == 0) {
        	fallbackMarker                = new Marker2D();
        	fallbackMarker.GlobalPosition = GlobalPosition;
        }
	}

	public void TimeShiftChange(bool isFuture) {
		if (staticSprite) return;
		spriteRef.SetTexture(isFuture ? FUTURE_SPRITE : PAST_SPRITE);
	}

	void StandingOn(Node2D body) {
		if (body is not Player player) return;
		player.EnteredPlatform(this, GetClosestAnchor(player.GlobalPosition).GlobalPosition);
	}

	void NotStandingOn(Node2D body) {
		if (body is not Player player) return;
		player.ExitPlatform(this);
	}

	public Marker2D GetClosestAnchor(Vector2 pos) {
		if (_anchors.Count == 0) {
			GD.PrintErr($"No anchors defined for {Name}. Falling back on global position.");
			return fallbackMarker;
		}
		
		Marker2D closest                = _anchors[0];
		float    closestDistanceSquared = pos.DistanceSquaredTo(closest.GlobalPosition);

		foreach (var anchor in _anchors) {
			float distanceSquared = pos.DistanceSquaredTo(anchor.GlobalPosition);
			if (distanceSquared < closestDistanceSquared) {
				closest                = anchor;
				closestDistanceSquared = distanceSquared;
			}
		}

		return closest;
	}
}