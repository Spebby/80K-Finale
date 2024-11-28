using System;
using Godot;
using System.Collections.Generic;
using System.Linq;
using Godot.Collections;

public partial class Room : Area2D, ITimeShiftable, IPauseable {
    // Get global positions of all camera anchors in each room. During a transition,
    // the player camera will interpolate its position from the closest anchor in
    // the old room to the closest anchor in the new room.
    List<IPauseable> Pauseables;
    CollisionShape2D bounds;

    [ExportGroup("Time Shift Settings")]
    Node2D TILESETS;
    [Export] CompressedTexture2D PAST_TILESET;
    [Export] CompressedTexture2D FUTURE_TILESET;
    [Export] Node2D PAST_OBJECT_REFERENCE;
    [Export] Node2D FUTURE_OBJECT_REFERENCE;

    List<Node> PastObjects;
    List<Node> FutureObjects;

    [Export] Array<Node2D> MiscTimeShifters;
    List<ITimeShiftable> SwapPalette;

    [ExportGroup("Event Groups")]
    [Export] BoolEventChannel onTimeShift;

    public override void _Ready() {
        SwapPalette = new List<ITimeShiftable>();
        GD.Print(MiscTimeShifters);
        foreach (var node in MiscTimeShifters) {
            if (node is ITimeShiftable timeShiftableNode) {
                GD.Print(node.Name, timeShiftableNode);
                SwapPalette.Add(timeShiftableNode);
                continue;
            }
            GD.PrintErr($"{Name} - Error: Node {node.Name} does not implement ITimeShiftable.");
            throw new InvalidOperationException($"Node {node.Name} does not implement ITimeShiftable.");
        }
        
        Pauseables = new List<IPauseable>();
        Pauseables.AddRange(GetNodeOrNull<MovingPlatformManager>("MovingPlatforms")?
                            .GetChildren()
                            .OfType<IPauseable>() ?? Enumerable.Empty<IPauseable>());


        TILESETS                   =  GetNode<Node2D>("Tilesets");
        bounds                     =  GetNode<CollisionShape2D>("Bounds");
        onTimeShift.OnEventTrigger += TimeShiftChange;
        BodyEntered                += Area2D_BodyEntered;
        
        PastObjects   = PAST_OBJECT_REFERENCE.GetChildren().ToList();
        FutureObjects = FUTURE_OBJECT_REFERENCE.GetChildren().ToList();
        
        // Calling to init everything right
        TimeShiftChange(false);
        
        if (TILESETS == null) {
            GD.PrintErr($"{this.Name} is missing child 'Tileset'!");
        }
    }

    // called via signal. 
    void Area2D_BodyEntered(Node2D body) {
        if (body is not Player) return;

        GD.Print("Player has entered the room!");
        ZoneCamera cam = body.GetNode<ZoneCamera>("Camera");
        cam.Transition(this);
    }

    public void TimeShiftChange(bool isFuture) {
        CompressedTexture2D newSet = isFuture ? FUTURE_TILESET : PAST_TILESET;
        SetTilesets(newSet);
        foreach (Node2D node in PastObjects) {
            node.Visible = !isFuture;
            node.SetProcessMode(!isFuture  ? ProcessModeEnum.Inherit : ProcessModeEnum.Disabled);
            if (node is TileMapLayer tileset) {
                tileset.Enabled = !isFuture;
            }
        }
        foreach (Node2D node in FutureObjects) {
            node.Visible = isFuture;
            node.SetProcessMode(isFuture ? ProcessModeEnum.Inherit : ProcessModeEnum.Disabled);
            if (node is TileMapLayer tileset) {
                tileset.Enabled = isFuture;
            }
        }
        
        // Swap the palletes on moving objects and other misc stuff
        SwapPalette.ForEach(shiftable => shiftable.TimeShiftChange(isFuture));
    }
    
    void SetTilesets(CompressedTexture2D newTexture) {
        foreach (var tile in TILESETS.GetChildren()
         .OfType<TileMapLayer>()
         .Select(layer => layer.TileSet?.GetSource(0) as TileSetAtlasSource)
         .Where(tile => tile != null)) 
        {
            tile.Texture = newTexture;
        }
    }

    public Vector2 GetDiagonal() => bounds.Shape.GetRect().Size;
    public Vector2 GetHalfDiagonal() => bounds.Shape.GetRect().End;

    public void Pause() => Pauseables.ForEach(p => p.Pause());
    public void Unpause() => Pauseables.ForEach(p => p.Unpause());

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