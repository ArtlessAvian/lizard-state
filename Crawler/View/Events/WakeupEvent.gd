extends "../EventHandler.gd"


func can_run_concurrently_with(handlers):
	return view.AnyActorAnimating() and len(handlers) == 0


func run():
	var subject = roles[event["subject"]]

	subject.get_node("AnimationPlayer").play("RESET")
	subject.get_node("AnimationPlayer").advance(0)
	subject.get_node("AnimationPlayer").play("Wakeup")
	print(subject.get_node("AnimationPlayer").current_animation)
