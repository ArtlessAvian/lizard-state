extends "../EventHandler.gd"


func should_wait_before():
	return view.AnyActorAnimating()


func run():
	var subject = roles[event["subject"]]

	var temp = event.args
	subject.FacePosition(temp)
	subject.SetAnimationTarget(temp - subject.tilePosition)

	var animation = subject.get_node("AnimationPlayer")
	var animation_name = "Attack"
	if event.has("flavorTags"):
		for tag in event["flavorTags"]:
			if animation.has_animation("Attack" + tag):
				animation_name = "Attack" + tag
				break

	animation.play("RESET")
	animation.advance(0)
	animation.play(animation_name)
	animation.advance(0)
