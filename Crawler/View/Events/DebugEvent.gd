extends "../EventHandler.gd"


func run():
	message_log.AddMessage("[color=#00ffff]" + event.args + "[/color]")
