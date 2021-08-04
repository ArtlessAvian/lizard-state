extends Resource

var actor_scene = preload("res://Crawler/View/Actor.tscn")

func _init(view, event : Dictionary, roles : Array):
	var entity = event["args"]

	# Find the actor, else, get a generic actor and try to recolor it i guess
	# TODO: don't make a new directory every time.
	var actor;
	actor = actor_scene.instance();
	actor.get_node("AnimatedSprite").frames = load("res://Crawler/View/Assets/ActorAtlas/" + entity.species.resource_name + ".tres")
	
	roles.append(actor)
	actor.name = str(entity.id);
	actor.ActAs(entity);

	view.find_node("Actors").add_child(actor);

	# HACK: lmao
	if entity.id == 0:
		view.playerActor = actor
		view.get_node("Camera2D").focus = actor
