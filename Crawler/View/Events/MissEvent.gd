extends "../EventHandler.gd"

var damage_popup_scene : PackedScene = preload("res://Crawler/View/DamagePopup.tscn")
var start_time

func should_wait_before():
	if is_same_subject():
		return false
	return view.AnyActorAnimating()

func run():
	start_time = now()
	var subject = roles[event["subject"]]
	var object = roles[event["object"]]

	subject.FacePosition(object.targetPosition)

	var animation = subject.get_node("AnimationPlayer")
	animation.play("Reset")
	animation.advance(0)
	animation.play("Attack")
	
	# var popup = damage_popup_scene.instance()
	# popup.text = "Miss"
	# popup.rect_position.y = object.get_node("DamagePopups").get_child_count() * -10
	# object.get_node("DamagePopups").add_child(popup)

func should_wait_after():
	return now() - start_time < 12 * (1000/60)
