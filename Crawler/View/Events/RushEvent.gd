extends "../EventHandler.gd"

var damage_popup_scene: PackedScene = preload("res://Crawler/View/Actor/DamagePopup.tscn")

var start_time


func should_wait_before():
	# if is_same_subject():
	# print("skipping wait")
	# if previous_event.action == "Rush":
	# return false
	return view.AnyActorAnimating()


func run():
	start_time = now()
	var subject = roles[event["subject"]]
	var object = roles[event["object"]]

	subject.FacePosition(object.tilePosition)
	subject.SetAnimationTarget(object.tilePosition - subject.tilePosition)

	var animation = subject.get_node("AnimationPlayer")
	animation.play("RESET")
	animation.advance(0)
	animation.play("Rush")

	object.health -= event["damage"]

	if object.status != null:
		object.status.set_health(object.health)

	var popup = damage_popup_scene.instance()
	popup.text = "-" + str(event["damage"])
	popup.rect_position.y = object.get_node("DamagePopups").get_child_count() * -10
	object.get_node("DamagePopups").add_child(popup)

	var otheranimation = object.get_node("AnimationPlayer")
	otheranimation.play("RESET")
	otheranimation.advance(0)
	otheranimation.play("Stunned" if object.stunned or object.health <= 0 else "Hurt")
	otheranimation.advance(0)

	object.get_node("HealthBar").value = object.health

	var hitEffect = object.get_node("AnimatedSprite/HitEffect")
	if hitEffect.frames.has_animation("Bash"):
		hitEffect.play("Bash")
