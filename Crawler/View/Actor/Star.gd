extends Sprite
tool

func _process(delta):
	self.position = (Vector2.RIGHT * 10).rotated(5)
