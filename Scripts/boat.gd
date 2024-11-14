extends Area2D
class_name Boat
@onready var sprite_2d = $Sprite2D

func set_texture(texture: Texture2D):
	sprite_2d.texture = texture
	
