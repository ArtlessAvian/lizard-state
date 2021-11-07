extends Resource

var damage_popup_scene : PackedScene = preload("res://Crawler/View/DamagePopup.tscn")

func _init(view, event : Dictionary, roles : Array):
	# print(event)
	var subject = roles[event["subject"]]
	# subject.FacePosition(event["targetPos"]);
	var animation = subject.get_node("AnimationPlayer");
	animation.play("Attack");
	
	do_hit_result(event["hit"], roles[event["object"]])
	

func do_hit_result(result : Dictionary, entity):
	entity.health -= result["damage"]
	entity.stunned = result["stuns"] or entity.stunned

	if entity.status != null:
		entity.status.set_health(entity.health)

	var popup = damage_popup_scene.instance();
	popup.parse_hit_result(result)
	entity.get_node("DamagePopups").add_child(popup)

	var animation = entity.get_node("AnimationPlayer")
	animation.play("Stunned" if entity.stunned else "Hurt")
