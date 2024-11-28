using System;
using Godot;
using System.Collections.Generic;

public partial class MovingPlatformManager : Node2D, IPauseable {
	// Called when the node enters the scene tree for the first time.
	List<PathManager> pathManagers;
	// keeping track of path managers is currently not needed, but in the future it might be.
	
	public override void _Ready() {
		pathManagers = new List<PathManager>();
		foreach (Node node in GetChildren()) {
			if (node is not PathManager path) {
				throw new Exception($"{Name} has non PathManager children!");
			}
			
			pathManagers.Add(path);
		}
	}


	public void Pause() {
		foreach (PathManager obj in pathManagers) {
			obj.Pause();
		}
	}

	public void Unpause() {
		foreach (PathManager obj in pathManagers) {
			obj.Unpause();
		}
	}
}