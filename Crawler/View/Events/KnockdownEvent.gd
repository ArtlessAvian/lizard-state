extends "../EventHandler.gd"


func run():
	var subject = roles[event["subject"]]

	var animation = subject.get_node("AnimationPlayer")
	animation.play("Swept")
	animation.advance(0)
