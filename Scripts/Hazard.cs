using Godot;

public partial class Hazard : Area2D {
	public override void _Ready() {
		BodyEntered +=  Area2D_BodyEntered;
	}

	void Area2D_BodyEntered(Node2D body) {
		if (body is not Player player) return;
		GD.PrintErr("Wooo");
		player.KillTimer(true);
	}
}