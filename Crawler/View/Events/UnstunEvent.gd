extends "../EventHandler.gd"


func run():
	var subject = roles[event["subject"]]
	# subject.stunned = false

	var aniPlayer = subject.get_node("AnimationPlayer")
	aniPlayer.play("RESET")

	subject.get_node("ComboBar").value = 0
