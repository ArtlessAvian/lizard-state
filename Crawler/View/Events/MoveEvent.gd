extends "../EventHandler.gd"

func can_accept_child(handler) -> bool:
	if handler.get_script() == self.get_script():
		if roles[handler.event["subject"]].movementTween != null and roles[handler.event["subject"]].movementTween.is_running():
			return false
		return true
	if handler.get_script() == load("res://Crawler/View/Events/SeeMapEvent.gd"):
		return true
	return false


func run():
	var subject = roles[event["subject"]]

	var from = subject.tilePosition
	var to = event["args"]

	subject.FacePosition(to)
	subject.GoToPosition(to, 15)

	# var animation = subject.get_node("AnimationPlayer");
	# animation.play("Move");
	# animation.advance(0);

	# subject.get_node("AnimationPlayer").play("RESET");

	# view.get_node("Map/Floors/Footsteps").set_cellv(from, 0)

	subject.get_node("ComboBar").value = 0


func is_done():
	var subject = roles[event["subject"]]

	# TODO: Replace with model suppression of unseen events.
	if not subject.seen:
		return true

	return subject.movementTween == null or not subject.movementTween.is_running()
