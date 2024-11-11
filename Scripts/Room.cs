using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class Room : Node2D {
    // Get global positions of all camera anchors in each room. During a transition,
    // the player camera will interpolate its position from the closest anchor in
    // the old room to the closest anchor in the new room.
    
    // If we want to interface w/ engine we'll need to use Godot.Array, but for local things,
    // use List<T>.
    public List<Vector2> GetCameraAnchors() => 
        (from Node2D anchor in GetNode("CameraAnchors").GetChildren() select anchor.GlobalPosition).ToList();

    public Vector2 GetClosestCameraAnchor(Player player) {
        Vector2 playerPos = player.GlobalPosition;

        float minDist = float.MaxValue;
        Vector2 minDistAnchor = new Vector2();

        foreach (Vector2 anchor in GetCameraAnchors()) {
            float dist = playerPos.DistanceTo(anchor);
            if (dist < minDist) {
                minDist = dist;
                minDistAnchor = anchor;
            }
        }

        return minDistAnchor;
    }

    // Get room dimensions based on the CollisionShape2D's extents.
    public Vector2 GetRoomDimensions() =>
        ((CollisionShape2D)GetNode("RoomBoundaries/CollisionShape2D")).Shape.GetRect().Size;

    public void Pause() {
        // do nothing for now
    }

    public void Resume() {
        // do nothing for now
    }
}
