extends "../EventHandler.gd"

var damage_popup_scene: PackedScene = preload("res://Crawler/View/Actor/DamagePopup.tscn")

var start_time


func can_run_concurrently_with(handlers):
	for handler in handlers:
		if handler.get_script() != self.get_script():
			return false
	return true


func run():
	start_time = now()

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

	add_message("{subject} spots a {object}!", "#ffa")


func is_done():
	return now() >= start_time + 1000 * 0.2
