extends "../EventHandler.gd"


func run():
	view.items[event["args"]].visible = false

	add_message("{subject} gets the item.", "#ffa")
