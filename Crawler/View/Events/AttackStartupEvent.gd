extends "../EventHandler.gd"

func run():
	var subject = roles[event["subject"]]
	
	var temp = event.args
	subject.FacePosition(temp)
	
	var animation = subject.get_node("AnimationPlayer")
	animation.play("Reset")
	animation.advance(0)