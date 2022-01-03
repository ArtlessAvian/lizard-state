extends "../EventHandler.gd"

func run():
	var subject = roles[event["subject"]]
	
	subject.seen = false
	# subject.get_node("AnimationPlayer").play("RESET");
