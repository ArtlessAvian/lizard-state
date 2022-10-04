extends "../EventHandler.gd"


func can_run_concurrently_with(handlers):
	for handler in handlers:
		if handler.get_script() != self.get_script():
			return false
		elif handler.event["subject"] == self.event["subject"]:
			return false
	return true


func run():
	var subject = roles[event["subject"]]

	var from = subject.tilePosition
	var to = event["args"]

	subject.FacePosition(to)
	subject.GoToPosition(to, 15)

	# var animation = subject.get_node("AnimationPlayer");
	# animation.play("Move");
	# animation.advance(0);

	# subject.get_node("AnimationPlayer").play("RESET");

	# view.get_node("Map/Floors/Footsteps").set_cellv(from, 0)

	subject.get_node("ComboBar").value = 0


func is_done():
	var subject = roles[event["subject"]]
	return subject.movementTween == null or not subject.movementTween.is_running()
