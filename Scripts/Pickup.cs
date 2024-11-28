using Godot;

public partial class Pickup : Area2D {
	AudioStreamPlayer2D _player;
	[Export] BitEventChannel onPickup;
	[Export(PropertyHint.Layers2DPhysics)] uint _layer { get; set; } = 0;
	
	public override void _Ready() {
		_player     ??= GetNode<AudioStreamPlayer2D>("AudioStreamPlayer2D");
		BodyEntered +=  Area2D_BodyEntered;
	}

	void Area2D_BodyEntered(Node2D body) {
		if (body is not Player player) return;
		
		_player?.Play();
		onPickup.TriggerEvent(_layer);
		QueueFree();
	}
}