extends Resource

func _init(view, event : Dictionary, roles : Array):
	view.get_node("Map").AddVision(event)

pass
