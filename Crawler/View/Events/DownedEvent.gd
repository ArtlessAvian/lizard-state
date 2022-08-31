extends "../EventHandler.gd"


func should_wait_before():
	return view.AnyActorAnimating()


func run():
	var subject = roles[event["subject"]]

	subject.get_node("AnimationPlayer").queue("Downed")
