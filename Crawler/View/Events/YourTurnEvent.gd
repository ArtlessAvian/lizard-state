extends Resource

func _init():
	pass

func run(view, event : Dictionary, roles : Array):
    view.find_node("WaitPrompt").visible = false

    view.get_node("Camera2D").focus = roles[event["subject"]]