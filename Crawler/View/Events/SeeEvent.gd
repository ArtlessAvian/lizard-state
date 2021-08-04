extends Resource

func _init(view, event : Dictionary, roles : Array):
	var subject = roles[event["subject"]]
	var object = roles[event["object"]]
	
	subject.FacePosition(object.targetPosition)
	object.seen = true

	# subject.get_node("AnimationPlayer").play("Reset");
