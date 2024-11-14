using Godot;

public partial class PathManager : Node2D {
	[Export] ObjectContainer vehicles;
	[Export] int count;
	[Export] double interval;
	[Export] Path2D path;

	MovingObject[] objectPool;

	public override async void _Ready() {
		if (path == null) {
			GD.PrintErr($"{Name}'s path reference is null!");
		}

		GD.Randomize();
		objectPool     = new MovingObject[count];
		for (int i = 0; i < count; i++) {
			objectPool[i] = vehicles.GetRandom().Instantiate<MovingObject>();
			if (!objectPool[i].Loop && count > 1) {
				GD.PrintErr($"Path Manager does not support ping-pong behaviour with more than 1 Moving Object. Object {objectPool[i]}; {objectPool[i].Name} responsible.");
			}
			path.AddChild(objectPool[i]);
			await ToSignal(GetTree().CreateTimer(interval), Timer.SignalName.Timeout);
		}
	}
}
