extends "../EventHandler.gd"


func can_run_concurrently_with(handlers):
	return view.AnyActorAnimating() and len(handlers) == 0


func run():
	var subject = roles[event["subject"]]

	subject.get_node("AnimationPlayer").play("RESET")
	subject.get_node("AnimationPlayer").advance(0)
	subject.get_node("AnimationPlayer").play("Downed")
	subject.get_node("AnimationPlayer").advance(0)

	# if false:  # all player characters are downed.
	if event["subject"] == 0:
		yield(view.get_tree().create_timer(1), "timeout")
		view.get_node("UILayer/MessageLog").anchor_top = 0
		view.get_node("UILayer/MessageLog").margin_top = 20
		view.get_node("UILayer/MessageLog/Background").color = Color.black

		view.done = true

	add_message("{subject} is downed!", "#f00")
