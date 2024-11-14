extends Node2D

class_name WaterLane

@onready var boat_scene: PackedScene = preload("res://Scenes/boat.tscn")

# does not damage player health
#1 for left to right, -1 right to left
@export var direction = -1
@export var boat_texture: Texture2D
@export var boat_count = 3
@export var distance_between_boats = 200
@export var speed = 200
@export var movement_x_limit = 480

var boats = []

func _ready():
	var start_point_x = movement_x_limit * direction
	
	for i in boat_count:
		var boat = boat_scene.instantiate() as Boat
		boat.position = Vector2(-movement_x_limit + distance_between_boats * i * - direction, 0)
		boat.scale = Vector2(1,1)
		#boat.area_entered.connect(on_player_enter_boat)
		add_child(boat)
		boat.set_texture(boat_texture)
		boats.append(boat)
		
func _process(delta):
	for boat in boats:
		var new_position_x = speed * delta * direction + boat.position.x
		if abs(new_position_x - movement_x_limit) < 1:
			boat.position.x = -movement_x_limit
		else:
			boat.position.x = new_position_x

#func on_player_enter_boat(area: Area2D):
#	if area is Player
