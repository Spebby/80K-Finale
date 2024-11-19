using Godot;

public partial class PathManager : Path2D {
	[Export] ObjectContainer vehicles;
	[Export] int count;
	[Export] double interval;

	MovingObject[] objectPool;

	public override async void _Ready() {
		GD.Randomize();
		objectPool     = new MovingObject[count];
		for (int i = 0; i < count; i++) {
			objectPool[i] = vehicles.GetRandom().Instantiate<MovingObject>();
			if (!objectPool[i].Loop && count > 1) {
				GD.PrintErr($"Path Manager does not support ping-pong behaviour with more than 1 Moving Object. Object {objectPool[i]}; {objectPool[i].Name} responsible.");
			}
			AddChild(objectPool[i]);
			await ToSignal(GetTree().CreateTimer(interval), Timer.SignalName.Timeout);
		}
	}
}
