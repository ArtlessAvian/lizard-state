extends "../EventHandler.gd"


func run():
	var subject = roles[event["subject"]]
	var object = roles[event["object"]]

	var temp = subject.tilePosition
	subject.FacePosition(object.tilePosition)
	subject.GoToPosition(object.tilePosition, 5)
	object.FacePosition(temp)
	object.GoToPosition(temp, 5)

	add_message("{subject} swaps with {object}.", "#ffa")


func is_done():
	var subject = roles[event["subject"]]
	var object = roles[event["object"]]
	return (
		(subject.movementTween == null or not subject.movementTween.is_running())
		and (object.movementTween == null or not object.movementTween.is_running())
	)
