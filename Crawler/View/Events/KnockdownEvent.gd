extends "../EventHandler.gd"

# TODO: Split from HitEvent in model and view.

const ATTACK_ACTIVE_EVENT = preload("res://Crawler/View/Events/AttackActiveEvent.gd")
const HIT_EVENT = preload("res://Crawler/View/Events/HitEvent.gd")
const KNOCKBACK_EVENT = preload("res://Crawler/View/Events/KnockbackEvent.gd")


func can_run_concurrently_with(handlers):
	for handler in handlers:
		if not handler.get_script() in [ATTACK_ACTIVE_EVENT, HIT_EVENT, KNOCKBACK_EVENT]:
			return false
	return true


func run():
	var subject = roles[event["subject"]]

	var animation = subject.get_node("AnimationPlayer")
	animation.play("Swept")
	animation.advance(0)
