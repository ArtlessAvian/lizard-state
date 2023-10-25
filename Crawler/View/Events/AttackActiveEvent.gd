extends "../EventHandler.gd"

var done = false


func run():
	var subject = roles[event["subject"]]

	var temp = event.args
	subject.FacePosition(temp)
	subject.SetAnimationTarget(temp - subject.tilePosition)

	var animation = subject.get_node("AnimationPlayer")
	var animation_name = "Attack"
	if event.has("flavorTags"):
		for tag in event["flavorTags"]:
			if animation.has_animation("Attack" + tag):
				animation_name = "Attack" + tag
				break

	animation.play("RESET")
	animation.advance(0)
	animation.play(animation_name)
	animation.advance(0)

	if not ("quiet" in event) or !event["quiet"]:
		add_message("{subject} attacks!")

	# wait for subject event to finish.
	subject.connect("attack_active", self, "on_subject_attack_active_signal")
	# in parallel, wait a second as a failsafe.
	yield(subject.get_tree().create_timer(1), "timeout")
	done = true


func on_subject_attack_active_signal():
	done = true


func is_done():
	return done
