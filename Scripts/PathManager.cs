using Godot;

public partial class PathManager : Path2D, IPauseable, ITimeShiftable {
	[Export] ObjectContainer vehicles;
	[Export] int count;
	[Export] double interval;
	[Export] bool overrideLoopBehavior;
	[Export] bool Loop;
	[Export] float speedMultipler = 1f;
	bool isFutureRef;

	MovingObject[] objectPool;

	public override async void _Ready() {
		GD.Randomize();
		objectPool = new MovingObject[count];
		for (int i = 0; i < count; i++) {
			objectPool[i] = vehicles.GetRandom().Instantiate<MovingObject>();
			if (!overrideLoopBehavior && !objectPool[i].Loop && count > 1) {
				GD.PrintErr($"{this.Name} - Path Manager does not support ping-pong behaviour with more than 1 Moving Object. Object {objectPool[i]}; {objectPool[i].Name} responsible.");
			}

			if (overrideLoopBehavior) {
				objectPool[i].Loop = Loop;
			}

			objectPool[i].Speed *= speedMultipler;
			AddChild(objectPool[i]);
			objectPool[i].TimeShiftChange(isFutureRef);

			if (!(interval > 0) || i == count - 1) continue;
			await ToSignal(GetTree().CreateTimer(interval), "timeout");
			GD.Print($"Waited for {interval} seconds.");
		}
	}

	public void TimeShiftChange(bool isFuture) {
		isFutureRef = isFuture;
		foreach (ITimeShiftable obj in GetChildren()) {
			obj.TimeShiftChange(isFuture);
		}
	}
	
	public void Pause() {
		foreach (IPauseable obj in GetChildren()) {
			obj.Pause();
		}
	}

	public void Unpause() {
		foreach (IPauseable obj in GetChildren()) {
			obj.Unpause();
		}
	}
}
