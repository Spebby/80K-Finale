using Godot;

public partial class MovingHazard : MovingObject {
	protected MovingHazard(Path2D path, float? speed) : base(path, speed) {
		CanStandOn = false;
	}
	
	[Export] CollisionShape2D KillZone;

	public bool CanKill() => KillZone != null;
}