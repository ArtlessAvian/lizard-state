extends "../EventHandler.gd"

var time

func setup():
	time = OS.get_system_time_msecs()

func should_wait_after():
	# TODO: waiting a frame, instead of a fixed time.
	# use an update with delta?
	return (OS.get_system_time_msecs() - time <= 16*4) and not Input.is_action_pressed("ui_accept")
