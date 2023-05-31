extends Label

const PATH = "res://ci-version.txt"

func _ready():
	var file = File.new()
	var err = file.open(PATH, File.READ)
	if err:
		file.close()
		return
	
	text = file.get_as_text()
	file.close()
