using Godot;

/*
 * Basic EventChannel that doesn't pass any data. 
 */

[GlobalClass]
public partial class BitEventChannel : Resource {
	[Signal] public delegate void OnEventTriggerEventHandler(uint bit);

	public void TriggerEvent(uint bit) => EmitSignal(SignalName.OnEventTrigger, bit);
}