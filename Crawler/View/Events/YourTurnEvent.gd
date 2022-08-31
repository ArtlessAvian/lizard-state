extends "../EventHandler.gd"


func should_wait_before():
	return view.AnyActorAnimating()


func run():
	view.find_node("WaitPrompt").visible = false

	view.get_node("Camera2D").focus = roles[event["subject"]]
