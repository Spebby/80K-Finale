using Godot;

public partial class Checkpoint : Area2D {
	[Export] Marker2D Point;

	public override void _Ready() {
		Point       ??= GetNode<Marker2D>("Point");
		BodyEntered +=  Area2D_BodyEntered;
	}

	void Area2D_BodyEntered(Node2D body) {
		if (body is not Player player) return;
		player.SetCheckpoint(Point);
	}
}