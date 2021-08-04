extends Resource

func _init(view, event : Dictionary, roles : Array):
	var subject = roles[event["subject"]]   
	subject.stunned = false

	var aniSprite = subject.get_node("AnimatedSprite")
	aniSprite.frame = 0;
