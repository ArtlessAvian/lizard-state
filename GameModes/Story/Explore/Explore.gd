extends Node2D

signal win

# Manages a playlist, a series of models.
# Has reference to the story state, but does not own it.

const CRAWLER_SCENE = preload("res://Crawler/Crawler.tscn")

var playlist


func start_playlist(the_playlist):
	playlist = the_playlist

	var old_crawler = get_node_or_null("Crawler")
	if old_crawler != null:
		remove_child(old_crawler)

	var crawler = CRAWLER_SCENE.instance()
	add_child(crawler)

	load_next_model(crawler, playlist.GetCurrentModel())


func load_next_model(crawler, model):
	if model == null:
		end_playlist()
		return

	# show transition

	# run the game.
	crawler.Model = model
	yield(crawler, "Done")

	# recur? kinda.
	# maybe instead do a one-shot signal and some funkyness.
	load_next_model(crawler, playlist.CreateNextModel(crawler.Model))


func end_playlist():
	print("You win!")

	var old_crawler = get_node_or_null("Crawler")
	if old_crawler != null:
		remove_child(old_crawler)

	# signal parent
	emit_signal("win")
