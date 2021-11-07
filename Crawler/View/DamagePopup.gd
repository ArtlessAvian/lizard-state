extends Label

const offset = Vector2(-40, 0) 

var timer : float;
var velocity : Vector2;

func parse_hit_result(result):
	# Changing font color makes it bold? weird.
	# self.add_color_override
	# self.set("custom_colors/font_color", Color(168, 168, 168))
	
	self.text = "-%s" % result["damage"]
	if result["stuns"]:
		# self.set("custom_colors/font_color", Color(255, 0, 0))
		self.text += "!";
	if not result["hit"]:
		self.text = "Miss"

func _ready():
	self.velocity = Vector2.UP * 20
	self.velocity += Vector2.RIGHT * 10 * (randf() * 2 - 1)

func _process(delta):
	var acc = Vector2.DOWN * delta * 40
	self.rect_position = self.rect_position + velocity * delta + 0.5 * acc * delta * delta
	self.velocity += acc

	timer += delta

	if timer > 1:
		queue_free()
