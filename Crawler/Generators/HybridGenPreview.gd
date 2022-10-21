extends Node2D
tool

export(Resource) var hybridGen
export(bool) var regenerate = false
export(Resource) var model
export(bool) var read_from_model = false


# Called when the node enters the scene tree for the first time.
func _process(delta):
	if regenerate and hybridGen != null:
		regenerate = false
		from_generator()

		model = load("res://Crawler/Model/Model.cs").new()
		hybridGen.Generate(model)

		from_model()

	if read_from_model and model != null:
		read_from_model = false
		from_model()


func from_generator():
	var density = $DiscreteNoise.DENSITY
	for x in range(-100, 100):
		for y in range(-100, 100):
			var tile = round(hybridGen.SampleNoise(x, y) * density * 0.8)
			tile = clamp(tile, 0, density - 1)
			$DiscreteNoise.set_cell(x, y, tile)


func from_model():
	$ModelMap.clear()
	model.map.DumpToTilemap($ModelMap)
