using System;
using Godot;
using System.Collections.Generic;

public partial class MovingPlatformManager : Node2D, IPauseable {
	// Called when the node enters the scene tree for the first time.
	List<IPauseable> pathManagers;
	// keeping track of path managers is currently not needed, but in the future it might be.
	
	public override void _Ready() {
		pathManagers = new List<IPauseable>();
		foreach (Node node in GetChildren()) {
			if (node is not IPauseable path) {
				GD.PrintErr($"{Name} has non Pausable children!");
				continue;
			}
			
			pathManagers.Add(path);
		}
	}


	public void Pause() {
		foreach (IPauseable obj in pathManagers) {
			obj.Pause();
		}
	}

	public void Unpause() {
		foreach (IPauseable obj in pathManagers) {
			obj.Unpause();
		}
	}
}