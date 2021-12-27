extends "../EventHandler.gd"

func run():
    view.find_node("WaitPrompt").visible = false

    view.get_node("Camera2D").focus = roles[event["subject"]]