extends Resource

func _init():
	pass

func run(view, event : Dictionary, roles : Array):
	var subject = roles[event["subject"]]   
	subject.stunned = false

	var aniPlayer = subject.get_node("AnimationPlayer")
	aniPlayer.play("Reset")

	subject.get_node("ComboBar").value = 0