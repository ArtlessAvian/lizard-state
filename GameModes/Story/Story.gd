extends Node2D

# Owner of the story_state.
# Either foregrounds the Explore if not null, or the Lobby if null.

export(Resource) var story_state = load("res://GameModes/Story/StoryState.gd").new()


func _ready():
	$Explore.start_playlist(load("res://GameModes/Story/Explore/Playlist/Failsafe.tres"))


func _process(delta):
	pass
	# $Walkabout.visible = story_state.playlist == null
	# $Explore.visible = story_state.playlist != null
