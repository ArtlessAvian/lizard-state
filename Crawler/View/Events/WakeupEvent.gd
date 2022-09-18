extends "../EventHandler.gd"


func should_wait_before():
	return view.AnyActorAnimating()


func run():
	var subject = roles[event["subject"]]

	subject.get_node("AnimationPlayer").play("RESET")
	subject.get_node("AnimationPlayer").advance(0)
	subject.get_node("AnimationPlayer").play("Wakeup")
	print("ligma")
	print(subject.get_node("AnimationPlayer").current_animation)
