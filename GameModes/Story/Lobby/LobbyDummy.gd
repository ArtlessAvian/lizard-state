extends Node

# A collection of ugly buttons.
# Aware of the story and story state. (get_parent() is ok.)


func _on_StartTestPlaylist_button_up():
	get_parent().story_state.playlist = load("res://GameModes/Story/Explore/Playlist/Failsafe.tres")
