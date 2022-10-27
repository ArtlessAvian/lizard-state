extends "../EventHandler.gd"


func run():
	var subject = roles[event["subject"]]

	var from = subject.tilePosition
	var to = event["args"]

	subject.FacePosition(to)
	subject.GoToPosition(to, 30)

	# var animation = subject.get_node("AnimationPlayer");
	# animation.play("Move");
	# animation.advance(0);

	# subject.get_node("AnimationPlayer").play("RESET");

	# view.get_node("Map/Floors/Footsteps").set_cellv(from, 0)

	subject.get_node("ComboBar").value = 0


func is_done():
	var subject = roles[event["subject"]]
	return subject.movementTween == null or not subject.movementTween.is_running()
