extends "../EventHandler.gd"

var start_time

func setup():
	start_time = now()

func should_wait_after():
	# TODO: waiting a frame, instead of a fixed time.
	# use an update with delta?
	return false;
	return (now() - start_time <= 16*4) and not Input.is_action_pressed("ui_accept")
