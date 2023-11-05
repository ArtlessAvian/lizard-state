extends "../EventHandler.gd"

# TODON'T: Merge with AttackActive.

var damage_popup_scene: PackedScene = preload("res://Crawler/View/Actor/DamagePopup.tscn")

func run():
	var subject = roles[event["subject"]]
	var object = roles[event["object"]]

	var hitEffect = subject.get_node("AnimatedSprite/HitEffect")
	hitEffect.play("default")
	if event.has("flavorTags") and event["flavorTags"] != null:
		for tag in event["flavorTags"]:
			# print("hit by a ", tag)
			if hitEffect.frames.has_animation(tag):
				hitEffect.play(tag)
				break

	subject.timeStop = 0 / 60.0
	object.timeStop = 0 / 60.0

	var otheranimation = subject.get_node("AnimationPlayer")
	otheranimation.play("Block")
	otheranimation.advance(0)

	add_message("Blocked by the {subject}!!", "#f44")
