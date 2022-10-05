extends "../EventHandler.gd"

var damage_popup_scene: PackedScene = preload("res://Crawler/View/Actor/DamagePopup.tscn")
var start_time


func can_run_concurrently_with(handlers):
	return view.AnyActorAnimating() and len(handlers) == 0


func run():
	var subject = roles[event["subject"]]
	var object = roles[event["object"]]

	subject.FacePosition(object.tilePosition)
	subject.SetAnimationTarget(object.tilePosition - subject.tilePosition)

	var animation = subject.get_node("AnimationPlayer")
	animation.play("RESET")
	animation.advance(0)
	animation.play("Miss")
