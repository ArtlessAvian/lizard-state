extends "../EventHandler.gd"

# TODON'T: Merge with AttackActive.

const ATTACK_ACTIVE_EVENT = preload("res://Crawler/View/Events/AttackActiveEvent.gd")
const HIT_EVENT = preload("res://Crawler/View/Events/HitEvent.gd")


func can_run_concurrently_with(handlers):
	for handler in handlers:
		if (
			handler.get_script() != ATTACK_ACTIVE_EVENT
			and handler.get_script() != HIT_EVENT
			and handler.get_script() != self.get_script()
		):
			return false
	return true


# TODO: Bug. If knockback taken before moving, the move will happen first, while this is yielding.
func run():
	var subject = roles[event["subject"]]

	var object = roles[event["object"]]
	object.GoToPosition(event["args"], 60)
