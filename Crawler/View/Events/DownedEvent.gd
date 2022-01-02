extends "../EventHandler.gd"

func run():
	var subject = roles[event["subject"]]

	subject.get_node("AnimationPlayer").queue("Downed")

	
func should_wait_after():
	return view.AnyActorAnimating()