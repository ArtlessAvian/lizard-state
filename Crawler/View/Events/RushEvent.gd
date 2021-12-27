extends Resource

var damage_popup_scene : PackedScene = preload("res://Crawler/View/DamagePopup.tscn")
var last_attacker = null

func _init():
	pass

# func should_wait_before(view, event : Dictionary):
# 	var subject = view.roles[event.subject]
# 	if last_attacker != subject:
# 		print("eeee")
# 		return !view.AnyActorAnimating()
# 	return false

func run(view, event : Dictionary, roles : Array):
	var subject = roles[event["subject"]]
	var object = roles[event["object"]]

	subject.FacePosition(object.targetPosition)

	var animation = subject.get_node("AnimationPlayer")
	animation.play("Reset")
	animation.advance(0)
	animation.play("Attack")	

	object.health -= event["damage"]

	if object.status != null:
		object.status.set_health(object.health)

	var popup = damage_popup_scene.instance();
	popup.text = "-" + str(event["damage"])
	popup.rect_position.y = object.get_node("DamagePopups").get_child_count() * -10
	object.get_node("DamagePopups").add_child(popup)

	var otheranimation = object.get_node("AnimationPlayer")
	otheranimation.play("Reset")
	otheranimation.advance(0)
	otheranimation.play("Stunned" if object.stunned else "Hurt")
	otheranimation.advance(0)

	object.get_node("HealthBar").value = object.health
