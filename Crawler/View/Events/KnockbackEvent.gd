extends "../EventHandler.gd"

func run():
	var subject = roles[event["subject"]]
	subject.targetPosition = event["args"]

		
func should_wait_after():
	return view.AnyActorAnimating()