extends "../EventHandler.gd"


func can_run_concurrently_with(handlers):
	return view.AnyActorAnimating() and len(handlers) == 0


func run():
	var subject = roles[event["subject"]]

	if event["subject"] == 0:
		view.get_node("UILayer/MessageLog").anchor_top = 0
		view.get_node("UILayer/MessageLog").margin_top = 20
		view.get_node("UILayer/MessageLog/Background").color = Color.black

		yield(view.get_tree().create_timer(1), "timeout")

		view.done = true
