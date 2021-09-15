extends VBoxContainer

func initialize(actor):
	actor.status = self
	set_health(actor.role.health, actor.role.species.maxHealth)
	
	var east_idle : AtlasTexture = actor.get_node("AnimatedSprite").frames.get_frame("East", 0)
	var texture : AtlasTexture = $HBoxContainer/Portrait.texture.duplicate() 
	texture.atlas = east_idle
	$HBoxContainer/Portrait.texture = texture
	return

func set_health(val, maxxx = -1):
	var health_bar = get_node("HBoxContainer/VBoxContainer/HealthBar")
	health_bar.value = val

	if maxxx != -1:
		health_bar.max_value = maxxx

	health_bar.get_node("Label").text = str(health_bar.value) + "/" + str(health_bar.max_value)

func set_energy(val, maxxx = -1):
	var bar = get_node("HBoxContainer/VBoxContainer/EnergyBar")
	bar.value = val

	if maxxx != -1:
		bar.max_value = maxxx

		bar.get_node("Label").text = str(bar.value) + "/" + str(bar.max_value)
