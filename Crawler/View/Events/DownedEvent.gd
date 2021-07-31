extends Node

func _init(event : Dictionary, roles : Array):
	var subject = roles[event["subject"]]

	subject.get_node("AnimationPlayer").queue("Downed")