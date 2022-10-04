extends "../EventHandler.gd"


func can_run_concurrently_with(handlers: Array) -> bool:
	return true
	

func run():
	view.get_node("Map").AddVision(event)


func is_done():
	return true
