extends Resource

var time

func _init():
	pass
	
func first_run(_view, event : Dictionary, roles : Array):
	time = OS.get_system_time_msecs()

func run(_view, event : Dictionary, roles : Array):
	pass

func can_consume():
	# TODO: waiting a frame, instead of a fixed time.
	# use an update with delta?
	return (OS.get_system_time_msecs() - time >= 16*4) or Input.is_action_pressed("ui_accept")
