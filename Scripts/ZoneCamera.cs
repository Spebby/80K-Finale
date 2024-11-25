using Godot;

public partial class ZoneCamera : Camera2D {
    Player _player; 
    Tween _tween;
    Vector2 _orgAnchor;

    [ExportGroup("Event Channels")]
    [Export] EventChannel OnPlayerDeath;

    public override void _Ready() {
        _player                      =  GetParent<Player>();
        _orgAnchor                   =  GlobalPosition;
        //OnPlayerDeath.OnEventTrigger += test;
    }

    Vector2 CalculateTransitionAnchor(Room room) {
        Vector2 playerPos   = _player.GlobalPosition;
        Vector2 camHalfDims = (GetViewportRect().Size / this.Zoom) / 2;
        Vector2 bHalfDiag   = room.GetHalfDiagonal();
        Vector2 bCent       = room.GetGlobalPosition();

        // diffs are positive if outside range on that cardinal
        float   uDiff  = (playerPos.Y + camHalfDims.Y) - (bCent.Y + bHalfDiag.Y);
        float   dDiff  = (bCent.Y - bHalfDiag.Y) - (playerPos.Y - camHalfDims.Y);
        float   lDiff  = (bCent.X - bHalfDiag.X) - (playerPos.X - camHalfDims.X);
        float   rDiff  = (playerPos.X + camHalfDims.X) - (bCent.X + bHalfDiag.X);
        Vector2 anchor = playerPos;
        
        // Previously, this code was using SDFs for calculations. But I needed to calculate the edge diffs
        // for secondary adjustments anyway... so unfortunately, the code is a lot less elegant than it was.

        if (lDiff > 0) anchor.X += lDiff;
        if (rDiff > 0) anchor.X -= rDiff;
        if (uDiff > 0) anchor.Y -= uDiff;
        if (dDiff > 0) anchor.Y += dDiff;

        return anchor;
    }

    public async void Transition(Room newRoom) {
        // Setup
        // Pause player processing (physics and input processing, animations, state timers, etc.)
        newRoom.Pause();
        _player.Pause();

        // Save the local position of the camera, so after we move it, we can restore the "anchor" of it to a sensible place.
        _orgAnchor = Position;
        /* The "position" of the camera =/= where it appears to be b/c of camera limits.
           we move the camera's position to where it "appears" to be, to prevent the camera snapping
           to the player the moment limits are removed. */
        Position   = ToLocal(GetScreenCenterPosition());

        // Disable smoothing to avoid jitter during transition.
        PositionSmoothingEnabled = false;
        
        // Main

        Vector2 endPoint = CalculateTransitionAnchor(newRoom);
        const float duration = 0.75f;

        RemoveCameraLimits();
        
        _tween = CreateTween();
        _tween.TweenProperty(this, "global_position", endPoint, duration)
              .SetTrans(Tween.TransitionType.Quad)
              .SetEase(Tween.EaseType.InOut);

        await ToSignal(_tween, Tween.SignalName.Finished);
        _tween.Kill();
        
        // Teardown
        // Restore local camera position to the original anchor point.
        Position = _orgAnchor;

        // Adjust camera limits to match room dimensions.
        FitCameraLimitsToRoom(newRoom);
        PositionSmoothingEnabled = true;

        // Restore player processing.
        newRoom.Unpause();
        _player.Unpause();
    }

    void FitCameraLimitsToRoom(Room room) {
        Vector4 limits = room.GetCardinalBounds();
        LimitTop    = (int)limits.X;
        LimitBottom = (int)limits.Y;
        LimitLeft   = (int)limits.Z;
        LimitRight  = (int)limits.W;

        GD.Print($"(T: {LimitTop}, B: {LimitBottom}, L: {LimitLeft}, R: {LimitRight})");
    }

    void RemoveCameraLimits() {
        LimitLeft   = -10000000;
        LimitRight  =  10000000;
        LimitTop    = -10000000;
        LimitBottom =  10000000;
    }
}
