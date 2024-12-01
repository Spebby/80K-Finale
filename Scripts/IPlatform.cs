using Godot;

public interface IPlatform {
    public Marker2D GetClosestAnchor(Vector2 pos);
}