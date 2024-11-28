using Godot;

public partial class FinalBoatManager : Path2D, IPauseable, ITimeShiftable {
	[Export] MovingObject boat;
	[Export] float speedMultipler = 1f;
	bool isFutureRef;
	float orgSpeedMult;

	Player player;
	public override async void _Ready() {
		orgSpeedMult   = speedMultipler;
		speedMultipler = 0;
		GD.Randomize();
		boat.Speed *= speedMultipler;
		AddChild(boat);
		boat.TimeShiftChange(isFutureRef);
		
		boat.GetCollider().BodyEntered += StartSequence;
		boat.OnProgressComplete        += EndSequence;
	}
	
	void StartSequence(Node2D body) {
		if (body is not Player player) return;
		this.player = player;
		player.EnteredPlatform(boat);
		player.Pause();
		speedMultipler                 =  orgSpeedMult;
		boat.Speed                     *= speedMultipler;
		
		// unsubscribe from future events so we can't retrigger start sequence.
		boat.GetCollider().BodyEntered -= StartSequence;
	}

	void EndSequence() {
		player.Unpause();
		speedMultipler =  0;
		boat.Speed     *= speedMultipler;
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
