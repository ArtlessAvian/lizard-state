extends "../EventHandler.gd"

const BLOCK_EVENT = preload("res://Crawler/View/Events/BlockEvent.gd")
const HIT_EVENT = preload("res://Crawler/View/Events/HitEvent.gd")
const KNOCKBACK_EVENT = preload("res://Crawler/View/Events/KnockbackEvent.gd")
const KNOCKDOWN_EVENT = preload("res://Crawler/View/Events/KnockdownEvent.gd")

var accept_children = false

func can_accept_child(child: Reference) -> bool:
	if accept_children and child.get_script() in [BLOCK_EVENT, HIT_EVENT, KNOCKBACK_EVENT, KNOCKDOWN_EVENT]:
		return true
	return false


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

	yield(subject, "attack_active")
	accept_children = true


func is_done():
	var subject = roles[event["subject"]]
	var animation: AnimationPlayer = subject.get_node("AnimationPlayer")
	return !animation.is_playing()
