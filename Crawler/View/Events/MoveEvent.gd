extends Resource

func _init(view, event : Dictionary, roles : Array):
	var subject = roles[event["subject"]]
	
	subject.FacePosition(event["args"])
	subject.targetPosition = event["args"]

	# subject.get_node("AnimationPlayer").play("Reset");
