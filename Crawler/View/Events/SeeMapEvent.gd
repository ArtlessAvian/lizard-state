extends Resource

func _init():
	pass

func run(view, event : Dictionary, roles : Array):
	view.get_node("Map").AddVision(event)

pass
