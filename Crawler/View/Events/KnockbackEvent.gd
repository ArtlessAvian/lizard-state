extends "../EventHandler.gd"


# TODO: Bug. If knockback taken before moving, the move will happen first, while this is yielding.
func run():
	var subject = roles[event["subject"]]

	yield(subject, "attack_active")

	var object = roles[event["object"]]
	object.GoToPosition(event["args"], 60)
