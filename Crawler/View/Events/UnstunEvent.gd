extends Resource

func _init(view, event : Dictionary, roles : Array):
	var subject = roles[event["subject"]]   
	subject.stunned = false

	var aniPlayer = subject.get_node("AnimationPlayer")
	aniPlayer.play("Reset")
