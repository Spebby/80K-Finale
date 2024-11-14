using Godot;

public partial class MovingObject : Area2D {
	[ExportGroup("Vehicle Properties")]
	[Export] protected float Speed = 64f;
	[Export] protected CollisionShape2D Bounds;
	[Export] protected bool CanStandOn;
	readonly protected Path2D Path;
	
	protected MovingObject(Path2D path, float? speed = null) {
		Path = path;
		// override SPEED if we define a new speed.
		Speed = speed ?? Speed;
	}

	public override void _Ready() {
		Bounds ??= GetNode<CollisionShape2D>("Bounds");
	}
	
	
}