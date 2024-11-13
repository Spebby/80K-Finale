using System;
using Godot;

public partial class ZoneCamera : Camera2D {
    Player _player; 
    Tween _tween;
    Vector2 _orgAnchor;

    public override void _Ready() {
        _player    = GetParent<Player>();
        _orgAnchor = GlobalPosition;
    }

    public async void Transition(Room newRoom) {
        TransitionSetup();

        // Find closest camera anchors in both previous and current room and use
        // their positions as interpolation points for camera position.
        // prevRoom = currRoom ?? newRoom; // if currRoom was unset, both should be newRoom!
        
        Vector2     endPoint = newRoom.GetClosestCameraAnchor(_player);
        GD.Print($"{GlobalPosition}, {endPoint}");
        const float duration = 0.5f;

        _tween = CreateTween();
        _tween.TweenProperty(this, "global_position", endPoint, duration)
              .SetTrans(Tween.TransitionType.Quad)
              .SetEase(Tween.EaseType.InOut);
        RemoveCameraLimits();

        await ToSignal(_tween, Tween.SignalName.Finished);
        _tween.Kill();
        TransitionTeardown(newRoom);
    }

    void InterpolateCamera(Vector2 endPoint) {
        
    }
    
    void TransitionSetup() {
        // Pause player processing (physics and input processing, animations, state timers, etc.)
        _player.Pause();

        // Save the original local position of the camera relative to the anchor.
        _orgAnchor = GlobalPosition;

        // Disable smoothing to avoid jitter during transition.
        PositionSmoothingEnabled = false;
    }

    void TransitionTeardown(Room room) {
        // Restore local camera position to the original anchor point.
        GlobalPosition = _orgAnchor;

        // Adjust camera limits to match room dimensions.
        FitCameraLimitsToRoom(room);
        PositionSmoothingEnabled = true;

        // Restore player processing.
        _player.Unpause();
    }
    
    void FitCameraLimitsToRoom(Room room) {
        Vector2 roomDims = room.GetHalfDiagonal();
        //GD.Print($"Dims:   {roomDims}, {room.GetDiagonal()}");
        //GD.Print($"Global: {room.GlobalPosition}");

        LimitTop    = (int)(room.GlobalPosition.Y - roomDims.Y);
        LimitBottom = (int)(room.GlobalPosition.Y + roomDims.Y);

        LimitLeft  = (int)(room.GlobalPosition.X - roomDims.X);
        LimitRight = (int)(room.GlobalPosition.X + roomDims.X);
        GD.Print($"(T: {LimitTop}, B: {LimitBottom}, L: {LimitLeft}, R: {LimitRight})");
    }

    void RemoveCameraLimits() {
        LimitLeft   = -10000000;
        LimitRight  =  10000000;
        LimitTop    = -10000000;
        LimitBottom =  10000000;
    }
}
