extends Resource

var view

func _init(_view, event : Dictionary, roles : Array):
	view = _view

func can_consume():
	return not view.AnyActorAnimating()
	# return OS.get_system_time_msecs() >= time + 0.5
