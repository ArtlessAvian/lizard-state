extends Node

func _init(event : Dictionary, roles : Array):
	var subject = roles[event["subject"]]
	
	subject.FacePosition(event["args"])
	subject.targetPosition = event["args"]

	# subject.get_node("AnimationPlayer").play("Reset");
