using Godot;
using Godot.Collections;
using System;

public partial class ObjectContainer : Resource {
	[Export] Array<PackedScene> Container;
	
	public PackedScene Get(int index) => this[index];
	public PackedScene GetRandom() => this[(int)GD.Randi() % Count()];

	public PackedScene this[int index] {
		get {
			if (index < 0 || index >= Container.Count) {
				throw new IndexOutOfRangeException(("Index out of range."));
			}
			return Container[index];
		}
	}

	public int Count() => Container.Count;
}
