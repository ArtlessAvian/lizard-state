extends "../EventHandler.gd"


func can_run_concurrently_with(handlers):
	return view.AnyActorAnimating() and len(handlers) == 0


func run():
	view.find_node("WaitPrompt").visible = false

	view.get_node("Camera2D").focus = roles[event["subject"]]


func is_done():
	return true
