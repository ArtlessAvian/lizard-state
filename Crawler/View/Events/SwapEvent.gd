extends "../EventHandler.gd"


func should_wait_before():
	var subject = roles[event["subject"]]
	return subject.movementTween != null and subject.movementTween.is_running()


func run():
	var subject = roles[event["subject"]]
	var object = roles[event["object"]]

	var temp = subject.tilePosition
	subject.FacePosition(object.tilePosition)
	subject.GoToPosition(object.tilePosition, 5)
	object.FacePosition(temp)
	object.GoToPosition(temp, 5)
