using Godot;

/*
 * Basic EventChannel that doesn't pass any data. 
 */

[GlobalClass]
public partial class EventChannel : Resource {
	[Signal] public delegate void OnEventTriggerEventHandler();

	public virtual void TriggerEvent() => EmitSignal(SignalName.OnEventTrigger);
}