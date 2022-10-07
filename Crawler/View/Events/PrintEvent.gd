extends "../EventHandler.gd"


func run():
	message_log.AddMessage("[color=#00aaaa]" + event.args + "[/color]")
