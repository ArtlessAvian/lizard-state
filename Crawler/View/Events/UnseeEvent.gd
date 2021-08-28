extends Resource

func _init(view, event : Dictionary, roles : Array):
	var subject = roles[event["subject"]]
	
	subject.seen = false
	# subject.get_node("AnimationPlayer").play("Reset");
