extends Resource

var damage_popup_scene : PackedScene = preload("res://Crawler/View/DamagePopup.tscn")

func _init():
	pass

func run(view, event : Dictionary, roles : Array):
	var subject = roles[event["subject"]]
	var animation = subject.get_node("AnimationPlayer")

	animation.play("Reset")
	animation.advance(0)
	animation.play("Attack")
	
	var object = roles[event["object"]]

	var popup = damage_popup_scene.instance()
	popup.text = "Miss"
	popup.rect_position.y = object.get_node("DamagePopups").get_child_count() * -10
	object.get_node("DamagePopups").add_child(popup)
