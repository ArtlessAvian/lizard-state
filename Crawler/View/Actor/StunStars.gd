extends Node2D
tool

export var stars: int = 3
export var period: float = 1 # seconds / revolution
export var fps_cap: int = 30 # frames / second
export var span: Vector2 = Vector2.ONE

func _process(delta):
	# sometimes some stars update position while others dont.
	# this looks kinda eh.
	# set fps cap to like a million to see.

	# round down to the nearest frame, then undo back to seconds.
	var discretized_time = floor(Time.get_ticks_msec() * fps_cap / 1000) / fps_cap

	if Engine.editor_hint:
		discretized_time = 0

	var base_theta = discretized_time * 2 * PI / period 

	if stars > self.get_child_count():
		stars = self.get_child_count()

	for i in range(stars):
		var sprite = get_child(i)
		var theta_offset = 2 * PI * i / stars

		sprite.position = span * Vector2.RIGHT.rotated(base_theta + theta_offset)
		if Engine.editor_hint:
			sprite.position = sprite.position.snapped(Vector2.ONE)
	
	for i in range(get_child_count()):
		get_child(i).visible = i < stars