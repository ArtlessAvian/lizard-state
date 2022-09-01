extends "../EventHandler.gd"


func run():
	var subject = roles[event["subject"]]
	var object = roles[event["object"]]

	var temp = subject.tilePosition
	subject.FacePosition(object.tilePosition)
	subject.GoToPosition(object.tilePosition, 15)
	object.FacePosition(temp)
	object.GoToPosition(temp, 15)
