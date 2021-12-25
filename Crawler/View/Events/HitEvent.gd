extends Resource

var damage_popup_scene : PackedScene = preload("res://Crawler/View/DamagePopup.tscn")

func _init(view, event : Dictionary, roles : Array):
	# print(event)
	var subject = roles[event["subject"]]
	# subject.FacePosition(event["targetPos"]);
	var animation = subject.get_node("AnimationPlayer");
	animation.play("Attack");
	
	do_hit_result(event["hit"], roles[event["object"]])
	
	# hack, will refactor hitting hopefully.
	roles[event["object"]].get_node("ComboBar").value = event["combo"]
	

func do_hit_result(result : Dictionary, actor):
	actor.health -= result["damage"]
	actor.stunned = result["stuns"] or actor.stunned

	if actor.status != null:
		actor.status.set_health(actor.health)

	var popup = damage_popup_scene.instance();
	popup.parse_hit_result(result)
	popup.rect_position.y = actor.get_node("DamagePopups").get_child_count() * -10
	actor.get_node("DamagePopups").add_child(popup)

	if result["hit"]:
		var animation = actor.get_node("AnimationPlayer")
		animation.play("Stunned" if actor.stunned else "Hurt")

	actor.get_node("HealthBar").value = actor.health
