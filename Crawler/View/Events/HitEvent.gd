extends "../EventHandler.gd"

var damage_popup_scene : PackedScene = preload("res://Crawler/View/DamagePopup.tscn")

func run():
	# var subject = roles[event["subject"]]
	# var animation = subject.get_node("AnimationPlayer");
	# animation.play("Attack");
	
	var object = roles[event["object"]]

	object.health -= event["damage"]
	object.stunned = true

	if object.status != null:
		object.status.set_health(object.health)

	var popup = damage_popup_scene.instance();
	popup.text = "-" + str(event["damage"])
	popup.rect_position.y = object.get_node("DamagePopups").get_child_count() * -10
	object.get_node("DamagePopups").add_child(popup)

	var otheranimation = object.get_node("AnimationPlayer")
	otheranimation.play("Stunned")

	object.get_node("HealthBar").value = object.health
