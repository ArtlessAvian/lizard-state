extends Label

var offset = Vector2(-40, 0)
var timer: float
var x_vel: float
# var velocity : Vector2;
export(Curve) var curve: Curve = null


func parse_hit_result(result):
	# Changing font color makes it bold? weird.
	# self.add_color_override
	# self.set("custom_colors/font_color", Color(168, 168, 168))

	self.text = "-%s" % result["damage"]
	if result["stuns"]:
		# self.set("custom_colors/font_color", Color(255, 0, 0))
		self.text += "!"
	if not result["hit"]:
		self.text = "Miss"


func _ready():
	offset = self.rect_position
	x_vel = randf() * 2 - 1


func _process(delta):
	self.rect_position = offset
	self.rect_position += Vector2.UP * curve.interpolate(timer) * 5
	self.rect_position += Vector2.RIGHT * min(0.75, timer) * 5 * x_vel

	timer += delta

	if timer > 1:
		queue_free()
