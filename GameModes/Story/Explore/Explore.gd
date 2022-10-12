extends Node2D

# Manages a playlist, a series of models.
# Has reference to the story state, but does not own it.

const CRAWLER_SCENE = preload("res://Crawler/Crawler.tscn")

var playlist


func start_playlist(the_playlist):
	playlist = the_playlist
	var crawler = CRAWLER_SCENE.instance()
	add_child(crawler)

	crawler.InitializeForReal(playlist.GetCurrentModel())

	yield(crawler, "Done")

	crawler.InitializeForReal(playlist.CreateNextModel(crawler.Model))

	yield(crawler, "Done")

	remove_child(crawler)
