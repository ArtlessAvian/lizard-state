extends "../EventHandler.gd"


func run():
	# very notable color intentionally.
	message_log.AddMessage("[color=#7fff00]" + event.args + "[/color]")
