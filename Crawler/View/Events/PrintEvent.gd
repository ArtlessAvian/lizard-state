extends "../EventHandler.gd"


func run():
	message_log.AddMessage("[color=#ddd]" + event.args + "[/color]")
