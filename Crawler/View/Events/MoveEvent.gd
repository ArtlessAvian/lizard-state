extends Resource

func _init():
	pass

func run(view, event : Dictionary, roles : Array):
	var subject = roles[event["subject"]]

	subject.FacePosition(event["args"])
	# subject.positionTwo = subject.targetPosition
	subject.targetPosition = event["args"]

	# var animation = subject.get_node("AnimationPlayer");
	# animation.play("Move");
	# animation.advance(0);

	# subject.get_node("AnimationPlayer").play("Reset");
