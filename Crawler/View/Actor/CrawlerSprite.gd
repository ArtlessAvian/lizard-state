extends AnimatedSprite
tool

const CANONICAL_SHEET = preload("res://Crawler/View/Assets/ActorAtlas/PlayerTegu.tres")

enum { IDLE, HURT, READY, DOWNED }

# Degrees: 0 is right, + is cw.
export(float) var facing_dir = 0.0
export(float) var facing_offset = 0.0

export(Vector2) var animation_target  # In Tiles
export(float) var sprite_lerp
export(float) var sprite_z


func _ready():
	if Engine.editor_hint:
		pass
	pass


# This should not rely on delta! All time stuff should be in the Actor or AnimationPlayer.
func _process(_delta):
	var dir = Vector2.RIGHT.rotated(deg2rad(facing_dir + facing_offset))
	var cur_frame = self.frame

	if abs(dir.y / dir.x) > 1:
		self.animation = "South" if dir.y > 0 else "North"
	else:
		self.animation = "East"
		self.flip_h = dir.x < 0

	self.frame = cur_frame

	var normalize = max(animation_target.x, animation_target.y)
	if normalize != 0:
		self.position = (animation_target / normalize) * Vector2(24, 16) * sprite_lerp
	else:
		self.position = Vector2.ZERO
	self.position += Vector2.UP * sprite_z
