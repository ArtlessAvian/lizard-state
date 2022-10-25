extends Node2D

# Owner of the story_state.
# Either foregrounds the Explore if not null, or the Lobby if null.

export(Resource) var story_state = load("res://GameModes/Story/StoryState.gd").new()


func _ready():
	if story_state.playlist != null:
		$Explore.start_playlist(story_state.playlist)


func _process(delta):
	$Walkabout.visible = story_state.playlist == null
	# TODO: this doesn't work properly because of nested viewports.
	# I suppose its fine, I just cleanup the crawler each time.
	$Explore.visible = story_state.playlist != null


func _unhandled_input(event):
	if event.is_action_pressed("ui_home"):
		ResourceSaver.save("res://save_state_story.tres", story_state)  #, ResourceSaver.FLAG_BUNDLE_RESOURCES
		print(story_state.playlist.currentModel)
	elif event.is_action_pressed("ui_end"):
		story_state = load("save_state_story.tres")
		_ready()  # this wont work.


func _on_Explore_win():
	story_state.playlist = null
