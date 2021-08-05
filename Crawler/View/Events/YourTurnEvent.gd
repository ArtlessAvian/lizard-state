extends Resource

func _init(view, event : Dictionary, roles : Array):
    view.find_node("WaitPrompt").visible = false
