extends "../EventHandler.gd"

# TODON'T: Merge with AttackActive.

var damage_popup_scene: PackedScene = preload("res://Crawler/View/Actor/DamagePopup.tscn")


func can_run_concurrently_with(handlers):
	for handler in handlers:
		if handler.get_script() != self.get_script():
			return false
	return true


func run():
	var subject = roles[event["subject"]]
	var object = roles[event["object"]]

	var hitEffect = object.get_node("AnimatedSprite/HitEffect")
	hitEffect.play("default")
	if event.has("flavorTags") and event["flavorTags"] != null:
		for tag in event["flavorTags"]:
			# print("hit by a ", tag)
			if hitEffect.frames.has_animation(tag):
				hitEffect.play(tag)
				break

	subject.timeStop = 0 / 60.0
	object.timeStop = 0 / 60.0

	object.health -= event["damage"]

	if object.status != null:
		object.status.set_health(object.health)

	var otheranimation = object.get_node("AnimationPlayer")
	if "swept" in event and event["swept"] == true:
		otheranimation.play("Swept")
	else:
		otheranimation.play("Stunned")
	otheranimation.advance(0)

	# yield(object.get_tree().create_timer(0.3), "timeout")

	var popup = damage_popup_scene.instance()
	popup.text = "-" + str(event["damage"])
	# TODO: Fix me.
	popup.rect_position.y = object.get_node("DamagePopups").get_child_count() * -10
	object.get_node("DamagePopups").add_child(popup)

	object.get_node("HealthBar").value = object.health

	add_message("Hits the {object}!!", "#f44")
