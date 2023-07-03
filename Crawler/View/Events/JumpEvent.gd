extends "../EventHandler.gd"

# Forked from AttackStartupEvent.
# Maybe can be genericized.

func can_run_concurrently_with(handlers):
	return view.AnyActorAnimating() and len(handlers) == 0


func run():
	var subject = roles[event["subject"]]

	var temp = event.args
	subject.FacePosition(temp)

	var animation = subject.get_node("AnimationPlayer")
	animation.play("RESET")
	animation.advance(0)
	animation.play("Jump")
	animation.advance(0)

	add_message("{subject} jumps!")
