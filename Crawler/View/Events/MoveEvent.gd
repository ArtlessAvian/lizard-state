extends "../EventHandler.gd"


func should_wait_before():
	var subject = roles[event["subject"]]
	return subject.movementTween != null and subject.movementTween.is_running()


func run():
	var subject = roles[event["subject"]]

	var from = subject.tilePosition
	var to = event["args"]

	subject.FacePosition(to)
	subject.GoToPosition(to, 15)
	subject.animationArg = Vector2.ZERO

	# var animation = subject.get_node("AnimationPlayer");
	# animation.play("Move");
	# animation.advance(0);

	# subject.get_node("AnimationPlayer").play("RESET");

	# view.get_node("Map/Floors/Footsteps").set_cellv(from, 0)

	subject.get_node("ComboBar").value = 0
