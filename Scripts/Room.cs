using Godot;

public partial class Room : Area2D {
    // Get global positions of all camera anchors in each room. During a transition,
    // the player camera will interpolate its position from the closest anchor in
    // the old room to the closest anchor in the new room.
    MovingPlatformManager movingPlatforms;
    
    Node2D TILESETS;
    CollisionShape2D bounds;

    [Export] CompressedTexture2D PAST_TILESET;
    [Export] CompressedTexture2D FUTURE_TILESET;

    [ExportGroup("Event Groups")]
    [Export] BoolEventChannel onTimeShift;

    public override void _Ready() {
        TILESETS = GetNode<Node2D>("Tilesets");
        if (TILESETS == null) {
            GD.PrintErr($"{this.Name} is missing child 'Tileset'!");
        }

        Node player = GetTree().GetFirstNodeInGroup("Player");
        if (player is not Player playerCast) {
            GD.PrintErr($"{player.Name} in group 'Player' is not type Player!");
            return;
        }

        movingPlatforms            =  GetNode<MovingPlatformManager>("MovingPlatforms");
        bounds                     =  GetNode<CollisionShape2D>("Bounds");
        onTimeShift.OnEventTrigger += TimeShiftChange;
        BodyEntered                += Area2D_BodyEntered;
    }

    // called via signal. 
    void Area2D_BodyEntered(Node2D body) {
        if (body is not Player) return;

        GD.Print("Player has entered the room!");
        ZoneCamera cam = body.GetNode<ZoneCamera>("Camera");
        cam.Transition(this);
    }

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

    public Vector2 GetDiagonal() => bounds.Shape.GetRect().Size;
    public Vector2 GetHalfDiagonal() => bounds.Shape.GetRect().End;

    public void Pause() => movingPlatforms.Pause();
    public void Unpause() => movingPlatforms.Unpause();

    public new Vector2 GetGlobalPosition() => bounds.GlobalPosition;
    
    /// <summary>
    /// Returns Cardinal Bounds of the Room's bounds in order Top, Bottom, Left, Right. Useful for setting camera boundries.
    /// </summary>
    public Vector4 GetCardinalBounds() {
        Vector2 roomDims    = GetHalfDiagonal();
        Vector2 roomCenter  = GetGlobalPosition();
        float limitTop    = roomCenter.Y - roomDims.Y;
        float limitBottom = roomCenter.Y + roomDims.Y;
        float limitLeft   = roomCenter.X - roomDims.X;
        float limitRight  = roomCenter.X + roomDims.X;
        return new Vector4(limitTop, limitBottom, limitLeft, limitRight);
    }
}