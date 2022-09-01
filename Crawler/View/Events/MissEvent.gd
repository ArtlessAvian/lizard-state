extends "../EventHandler.gd"

var damage_popup_scene: PackedScene = preload("res://Crawler/View/DamagePopup.tscn")
var start_time


func should_wait_before():
	return view.AnyActorAnimating()


func run():
	start_time = now()
	var subject = roles[event["subject"]]
	yield(subject, "attack_active")

	var object = roles[event["object"]]

	subject.FacePosition(object.tilePosition)
	subject.animationArg = object.tilePosition - subject.tilePosition

	var animation = subject.get_node("AnimationPlayer")
	animation.play("RESET")
	animation.advance(0)
	animation.play("Miss")

	# var popup = damage_popup_scene.instance()
	# popup.text = "Miss"
	# popup.rect_position.y = object.get_node("DamagePopups").get_child_count() * -10
	# object.get_node("DamagePopups").add_child(popup)
