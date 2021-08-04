extends Resource

var damage_popup_scene : PackedScene = preload("res://Crawler/View/DamagePopup.tscn")

func _init(view, event : Dictionary, roles : Array):
	# print(event)
	var subject = roles[event["subject"]]
	subject.FacePosition(event["targetPos"]);
	var animation = subject.get_node("AnimationPlayer");
	animation.play("Attack");
	
	var result = event["hit"]
	do_hit_result(result, roles[result["target"]])

	

func do_hit_result(result : Dictionary, entity):
	entity.health -= result["damage"]
	entity.stunned = result["stuns"] or entity.stunned
	entity.get_node("HealthBar").value = entity.health

	var popup = damage_popup_scene.instance();
	popup.parse_hit_result(result)
	entity.get_node("DamagePopups").add_child(popup)

	var animation = entity.get_node("AnimationPlayer")
	animation.play("Stunned" if entity.stunned else "Hurt")
