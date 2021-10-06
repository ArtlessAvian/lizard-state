extends Resource

func _init(view, event : Dictionary, roles : Array):
	var subject = roles[event["subject"]]

	var temp = event.args
	print(temp)
	subject.FacePosition(temp)
