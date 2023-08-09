extends Node

# A collection of ugly buttons.
# Aware of the story and story state. (get_parent() is ok.)


func _on_StartTutorial_button_up():
	get_parent().story_state.playlist = load("res://BaseGame/Playlists/Tutorial.tres").duplicate()
	get_node("../Explore").start_playlist(get_parent().story_state.playlist)


func _on_StartEarlygame_button_up():
	get_parent().story_state.playlist = load("res://BaseGame/Playlists/Midgame.tres").duplicate()
	get_node("../Explore").start_playlist(get_parent().story_state.playlist)


func _on_StartMidgame_button_up():
	get_parent().story_state.playlist = load("res://BaseGame/Playlists/Tutorial.tres").duplicate()
	get_node("../Explore").start_playlist(get_parent().story_state.playlist)
