extends Resource

func _init():
	pass

func run(view, event : Dictionary, roles : Array):
	var subject = roles[event["subject"]]
	
	subject.seen = false
	# subject.get_node("AnimationPlayer").play("Reset");
