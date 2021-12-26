extends Resource

func _init():
	pass

func run(view, event : Dictionary, roles : Array):
	var subject = roles[event["subject"]]
	var object = roles[event["object"]]

	var temp = subject.targetPosition
	subject.FacePosition(object.targetPosition)
	subject.targetPosition = object.targetPosition
	object.FacePosition(temp)
	object.targetPosition = temp
