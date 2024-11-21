using Godot;

public partial class PathManager : Path2D {
	[Export] ObjectContainer vehicles;
	[Export] int count;
	[Export] double interval;
	[Export] bool overrideLoopBehavior;
	[Export] bool Loop;
	[Export] float speedMultipler = 1f;

	MovingObject[] objectPool;

	public override async void _Ready() {
		GD.Randomize();
		objectPool = new MovingObject[count];
		for (int i = 0; i < count; i++) {
			objectPool[i] = vehicles.GetRandom().Instantiate<MovingObject>();
			if (!overrideLoopBehavior && !objectPool[i].Loop && count > 1) {
				GD.PrintErr($"Path Manager does not support ping-pong behaviour with more than 1 Moving Object. Object {objectPool[i]}; {objectPool[i].Name} responsible.");
			}

			objectPool[i].Loop  =  Loop;
			objectPool[i].Speed *= speedMultipler;
			AddChild(objectPool[i]);

			if (!(interval > 0) || i == count - 1) continue;
			await ToSignal(GetTree().CreateTimer(interval), "timeout");
			GD.Print($"Waited for {interval} seconds.");
		}
	}

	public void Pause() {
		foreach (MovingObject mov in GetChildren()) {
			mov.Pause();
		}
	}

	public void Unpause() {
		foreach (MovingObject mov in GetChildren()) {
			mov.Unpause();
		}
	}
}
