extends "../EventHandler.gd"

var actor_scene = preload("res://Crawler/View/Actor.tscn")
var status_scene = preload("res://Crawler/View/PartyMemberStatus.tscn")


func run():
	var entity = event["args"]

	# Find the actor, else, get a generic actor and try to recolor it i guess
	# TODO: don't make a new directory every time.
	var actor = actor_scene.instance();
	actor.get_node("AnimatedSprite").frames = load("res://Crawler/View/Assets/ActorAtlas/" + entity.species.resource_name + ".tres")
	
	roles.append(actor)
	actor.name = str(entity.id);
	actor.ActAs(entity);

	view.find_node("Actors").add_child(actor);

	# HACK: lmao
	if entity.id == 0:
		view.playerActor = actor
		view.get_node("Camera2D").focus = actor
	
	# Create status if allied
	if entity.team == 0:
		var status = status_scene.instance()
		status.initialize(actor)

		var party_status = view.find_node("PartyStatus")
		party_status.add_child(status)
