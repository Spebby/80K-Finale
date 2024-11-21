using Godot;

/*
 * Basic EventChannel that doesn't pass any data. 
 */

[GlobalClass]
public partial class BoolEventChannel : Resource {
	[Signal] public delegate void OnEventTriggerEventHandler(bool flag);

	public void TriggerEvent(bool flag) => EmitSignal(SignalName.OnEventTrigger, flag);
}