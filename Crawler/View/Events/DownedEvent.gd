extends Resource

func _init(view, event : Dictionary, roles : Array):
	var subject = roles[event["subject"]]

	subject.get_node("AnimationPlayer").queue("Downed")