extends "../EventHandler.gd"


func run():
	var subject = roles[event["subject"]]

	var from = subject.targetPosition
	var to = event["args"]

	subject.FacePosition(to)
	subject.targetPosition = to
	subject.animationArg = Vector2.ZERO

	# var animation = subject.get_node("AnimationPlayer");
	# animation.play("Move");
	# animation.advance(0);

	# subject.get_node("AnimationPlayer").play("RESET");

	# view.get_node("Map/Floors/Footsteps").set_cellv(from, 0)
