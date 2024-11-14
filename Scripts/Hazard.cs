using Godot;

public partial class Hazard : Area2D {
	[Export] CollisionShape2D KillBox;

	public override void _Ready() {
		KillBox     ??= GetNode<CollisionShape2D>("KillBox");
		BodyEntered +=  Area2D_BodyEntered;
	}

	void Area2D_BodyEntered(Node2D body) {
		if (body is not Player player) return;
		player.Kill();
	}
}