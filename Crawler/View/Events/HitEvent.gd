extends "../EventHandler.gd"

var damage_popup_scene: PackedScene = preload("res://Crawler/View/DamagePopup.tscn")


func run():
	var subject = roles[event["subject"]]
	var object = roles[event["object"]]

	var hitEffect = object.get_node("AnimatedSprite/HitEffect")
	hitEffect.play("default")
	if event.has("flavorTags") and event["flavorTags"] != null:
		for tag in event["flavorTags"]:
			print("hit by a ", tag)
			if hitEffect.frames.has_animation(tag):
				hitEffect.play(tag)
				break

	yield(subject, "attack_active")
	subject.timeStop = 0 / 60.0
	object.timeStop = 0 / 60.0

	object.health -= event["damage"]
	object.stunned = true

	if object.status != null:
		object.status.set_health(object.health)

	var otheranimation = object.get_node("AnimationPlayer")
	otheranimation.play("Stunned")
	otheranimation.advance(0)

	yield(object.get_tree().create_timer(0.3), "timeout")

	var popup = damage_popup_scene.instance()
	popup.text = "-" + str(event["damage"])
	# TODO: Fix me.
	popup.rect_position.y = object.get_node("DamagePopups").get_child_count() * -10
	object.get_node("DamagePopups").add_child(popup)

	object.get_node("HealthBar").value = object.health
