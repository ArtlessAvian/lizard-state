extends Node

func _init(event : Dictionary, roles : Array):
	var subject = roles[event["subject"]]   
	subject.stunned = false

	var aniSprite = subject.get_node("AnimatedSprite")
	aniSprite.frame = 0;
