extends "../EventHandler.gd"


func should_wait_before():
	var subject = roles[event["subject"]]
	return subject.movementTween != null and subject.movementTween.is_running()


func can_run_concurrently_with(handlers: Array) -> bool:
	for handler in handlers:
		if handler.get_script() != self.get_script():
			print(handler.get_script(), self.get_script())
			return false
	return true


func run():
	done = false
	run_all_subevents_except([])

	var subject = roles[event["subject"]]

	var from = subject.tilePosition
	var to = event["args"]

	subject.FacePosition(to)
	subject.GoToPosition(to, 15)

	# var animation = subject.get_node("AnimationPlayer");
	# animation.play("Move");
	# animation.advance(0);

	# subject.get_node("AnimationPlayer").play("RESET");

	# view.get_node("Map/Floors/Footsteps").set_cellv(from, 0)

	subject.get_node("ComboBar").value = 0

	yield(view.get_tree().create_timer(.5), "timeout")
	done = true
