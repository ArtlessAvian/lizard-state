extends Resource

var time

func _init(_view, event : Dictionary, roles : Array):
	time = OS.get_system_time_msecs()

func can_consume():
	# TODO: waiting a frame, instead of a fixed time.
	# use an update with delta?
	return OS.get_system_time_msecs() - time >= 16
