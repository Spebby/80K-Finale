using Godot;
using System.Collections.Generic;
using Godot.Collections;

public partial class VehicleContainer : Resource {
	[Export] public Array<MovingHazard> Vehicles;
}
