using Godot;
using System.Collections.Generic;
using System.Linq;

public partial class PieceBridge : Node2D {
	List<Area2D> platforms;
	uint active;
	
	[Export] bool offByDefault;
	[Export] BitEventChannel Channel;
	public override void _Ready() {
		platforms              =  GetChildren().OfType<Area2D>().ToList();
		Channel.OnEventTrigger += TogglePlatform;

		if (offByDefault) {
			active = 0;
			foreach (Area2D platform in platforms) {
				platform.Visible = false;
				platform.SetProcessMode(ProcessModeEnum.Disabled);
			}
		} else {
			active = ~((uint)(1 << (platforms.Count + 1)));
			foreach (Area2D platform in platforms) {
				platform.Visible = true;
				platform.SetProcessMode(ProcessModeEnum.Inherit);
			}
		}
	}

	void TogglePlatform(uint mask) {
		for (int i = 0; i < platforms.Count; i++) {
			if ((mask & (1 << i)) != 0) {
				bool status = (active & (i << i)) != 0;
				platforms[i].Visible = !status;
				platforms[i].SetProcessMode(!status  ? ProcessModeEnum.Inherit : ProcessModeEnum.Disabled);
			}
		}
		active ^= mask;
	}
}
