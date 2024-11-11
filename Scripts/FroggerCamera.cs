using Godot;
using System;
using System.Threading.Tasks;

public partial class FroggerCamera : Camera2D {
    [Signal]
    public delegate void TransitionCompletedEventHandler();

    Player _player; 
    Tween _tween;
    Vector2 _originalLocalAnchorPos;

    public override void _Ready() {
        _player = (Player)GetParent().GetParent();
        _tween = GetNode<Tween>("PositionTween");
    }

    public async void Transition(Room oldRoom, Room newRoom) {
        TransitionSetup();

        // Find closest camera anchors in both previous and current room and use
        // their positions as interpolation points for camera position.
        Vector2 oldGlobalPos = oldRoom.GetClosestCameraAnchor(_player);
        Vector2 newGlobalPos = newRoom.GetClosestCameraAnchor(_player);
        InterpolateCameraPos(oldGlobalPos, newGlobalPos);

        // Wait until the tween has started to avoid jitter.
        await ToSignal(_tween, "tween_started");
        RemoveCameraLimits();

        await ToSignal(_tween, "tween_completed");
        TransitionTeardown(newRoom);

        EmitSignal(SignalName.TransitionCompleted);
    }

    public void FitCameraLimitsToRoom(Room room) {
        Vector2 roomDims = room.GetRoomDimensions();

        LimitLeft = (int)room.GlobalPosition.X;
        LimitRight = (int)(room.GlobalPosition.X + roomDims.X);
        LimitTop = (int)room.GlobalPosition.Y;
        LimitBottom = (int)(room.GlobalPosition.Y + roomDims.Y);
    }

    private void TransitionSetup() {
        // Pause player processing (physics and input processing, animations, state timers, etc.)
        _player.Pause();

        // Save the original local position of the camera relative to the anchor.
        _originalLocalAnchorPos = Position;

        // Disable smoothing to avoid jitter during transition.
        PositionSmoothingEnabled = false;
    }

    private void TransitionTeardown(Room room) {
        // Restore local camera position to the original anchor point.
        Position = _originalLocalAnchorPos;

        // Adjust camera limits to match room dimensions.
        FitCameraLimitsToRoom(room);

        // Re-enable smoothing now that the transition has completed.
        PositionSmoothingEnabled = true;

        // Restore player processing.
        _player.Unpause();
    }

    private void InterpolateCameraPos(Vector2 oldGlobalPos, Vector2 newGlobalPos) {
        const string               property = "position";
        const float                duration = 0.50f;
        const Tween.TransitionType trans    = Tween.TransitionType.Quad;
        const Tween.EaseType       easing   = Tween.EaseType.InOut;

        // Convert tween start and end position from global to local coordinates.
        Vector2 oldLocal = ToLocal(oldGlobalPos);
        Vector2 newLocal = ToLocal(newGlobalPos);

        _tween.Stop();
        _tween.TweenProperty(this, property, newLocal, duration).SetTrans(trans).SetEase(easing);
        _tween.Play();
    }
    
    private void RemoveCameraLimits() {
        LimitLeft   = -10000000;
        LimitRight  =  10000000;
        LimitTop    = -10000000;
        LimitBottom =  10000000;
    }
}