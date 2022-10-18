extends Node2D

# Owner of the story_state.
# Either foregrounds the Explore if not null, or the Lobby if null.

export(Resource) var story_state = load("res://GameModes/Story/StoryState.gd").new()


func _ready():
	if story_state.playlist == null:
		story_state.playlist = load("res://GameModes/Story/Explore/Playlist/Failsafe.tres").duplicate()
	$Explore.start_playlist(story_state.playlist)


func _process(delta):
	pass
	# $Walkabout.visible = story_state.playlist == null
	# $Explore.visible = story_state.playlist != null


func _unhandled_input(event):
	if event.is_action_pressed("ui_home"):
		ResourceSaver.save("res://save_state_story.tres", story_state)  #, ResourceSaver.FLAG_BUNDLE_RESOURCES
	elif event.is_action_pressed("ui_end"):
		story_state = load("save_state_story.tres")
		_ready()  # this wont work.
