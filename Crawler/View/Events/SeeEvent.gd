extends "../EventHandler.gd"

var damage_popup_scene: PackedScene = preload("res://Crawler/View/DamagePopup.tscn")


func run():
	var subject = roles[event["subject"]]
	var object = roles[event["object"]]
	object.seen = true

	if subject == object:
		return

	subject.FacePosition(object.tilePosition)

	var popup = damage_popup_scene.instance()
	popup.text = "!"
	subject.get_node("DamagePopups").add_child(popup)

	# subject.get_node("AnimationPlayer").play("RESET");
