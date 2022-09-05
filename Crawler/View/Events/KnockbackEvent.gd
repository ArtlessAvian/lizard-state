extends "../EventHandler.gd"


func run():
	var subject = roles[event["subject"]]

	yield(subject, "attack_active")

	var object = roles[event["object"]]
	object.GoToPosition(event["args"], 60)
