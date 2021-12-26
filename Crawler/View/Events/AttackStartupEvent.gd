extends Resource

func _init(view, event : Dictionary, roles : Array):
	var subject = roles[event["subject"]]
	var animation = subject.get_node("AnimationPlayer")

	animation.play("Reset")
	animation.advance(0)