extends "../EventHandler.gd"


func run():
	var subject = roles[event["subject"]]
	subject.GoToPosition(event["args"], 60)
