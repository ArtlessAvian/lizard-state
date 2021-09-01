extends VBoxContainer

func initialize(actor):
	actor.status = self
	set_health(actor.role.health, actor.role.species.maxHealth)
	return

func set_health(val, maxxx = -1):
	var health_bar = get_node("HBoxContainer/HealthBar")
	health_bar.value = val

	if maxxx != -1:
		health_bar.max_value = maxxx

	health_bar.get_node("Label").text = str(health_bar.value) + "/" + str(health_bar.max_value)
