extends "../EventHandler.gd"


func run():
	var subject = roles[event["subject"]]

	var temp = event.args
	subject.FacePosition(temp)
	subject.animationArg = temp - subject.tilePosition

	var animation = subject.get_node("AnimationPlayer")
	animation.play("RESET")
	animation.advance(0)
	animation.play("Attack")
	animation.advance(0)
