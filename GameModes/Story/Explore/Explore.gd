extends Node2D

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

	crawler.Model = playlist.GetCurrentModel()

	yield(crawler, "Done")

	crawler.Model = playlist.CreateNextModel(crawler.Model)

	yield(crawler, "Done")

	remove_child(crawler)
