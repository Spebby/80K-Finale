using Godot;

public partial class Pickup : Area2D {
	AudioStreamPlayer2D _player;
	public override void _Ready() {
		_player     ??= GetNode<AudioStreamPlayer2D>("AudioStreamPlayer2D");
		BodyEntered +=  Area2D_BodyEntered;
	}

	void Area2D_BodyEntered(Node2D body) {
		if (body is not Player player) return;
		
		_player?.Play();
		Hide();
	}
}