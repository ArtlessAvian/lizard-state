extends Resource

func _init():
	pass

func run(view, event : Dictionary, roles : Array):
	var subject = roles[event["subject"]]
	var animation = subject.get_node("AnimationPlayer")

	animation.play("Reset")
	animation.advance(0)