using Godot;
using System.Collections.Generic;
using System.Linq;

public partial class Room : Area2D {
    // Get global positions of all camera anchors in each room. During a transition,
    // the player camera will interpolate its position from the closest anchor in
    // the old room to the closest anchor in the new room.
    MovingPlatformManager movingPlatforms;
    
    // anchors
    Node2D ANCHORS;
    Node2D TILESETS;

    [Export] CompressedTexture2D PAST_TILESET;
    [Export] CompressedTexture2D FUTURE_TILESET;

    [ExportGroup("Event Groups")]
    [Export] BoolEventChannel onTimeShift;

    public override void _Ready() {
        ANCHORS = GetNode<Node2D>("CameraAnchors");
        if (ANCHORS == null) {
            GD.PrintErr($"{this.Name} is missing child 'CameraAnchors'!");
        } else if (GetCameraAnchors().Count == 0) {
            GD.PrintErr($"{this.Name} {ANCHORS.Name} is missing anchor positions!");
        }

        TILESETS = GetNode<Node2D>("Tilesets");
        if (TILESETS == null) {
            GD.PrintErr($"{this.Name} is missing child 'Tileset'!");
        }

        Node player = GetTree().GetFirstNodeInGroup("Player");
        if (player is not Player playerCast) {
            GD.PrintErr($"{player.Name} in group 'Player' is not type Player!");
            return;
        }

        movingPlatforms = GetNode<MovingPlatformManager>("MovingPlatforms");

        onTimeShift.OnEventTrigger += TimeShiftChange;
        BodyEntered                += Area2D_BodyEntered;
    }

    // called via signal. 
    void Area2D_BodyEntered(Node2D body) {
        if (body is not Player) return;

        GD.Print("Player has entered the room!");
        ZoneCamera cam = body.GetNode<ZoneCamera>("Camera");
        cam.Transition(this);
        // wanted to make my own signal; but I don't understand the Observer pattern well enough
        // to make a well informed choice about its usage + I have limited time here and
        // this works fine for our use case.
    }

    // If we want to interface w/ engine we'll need to use Godot.Array, but for local things,
    // use List<T>.
    public List<Vector2> GetCameraAnchors() => 
        (from Node2D anchor in ANCHORS.GetChildren() select anchor.GlobalPosition).ToList();

    void TimeShiftChange(bool isFuture) {
        CompressedTexture2D newSet = isFuture ? FUTURE_TILESET : PAST_TILESET;
        SetTilesets(newSet);
    }
    
    void SetTilesets(CompressedTexture2D newTexture) {
        foreach (var node in TILESETS.GetChildren()) {
            // may break at some point.
            TileSetSource tile = ((TileMapLayer)node).TileSet.GetSource(0);
            if (tile is TileSetAtlasSource) {
                (tile as TileSetAtlasSource).Texture = newTexture;
            }
        }
    }

    public Vector2 GetClosestCameraAnchor(Player player) {
        Vector2 playerPos = player.GlobalPosition;
        return GetCameraAnchors()
               .OrderBy(anchor => playerPos.DistanceSquaredTo(anchor)) // Use squared distance for better performance
               .FirstOrDefault();
    }

    // Get room dimensions based on the CollisionShape2D's extents.
    public Vector2 GetDiagonal() => GetNode<CollisionShape2D>("Bounds").Shape.GetRect().Size;
    public Vector2 GetHalfDiagonal() => GetNode<CollisionShape2D>("Bounds").Shape.GetRect().End;

    public void Pause() => movingPlatforms.Pause();
    public void Unpause() => movingPlatforms.Unpause();
}
