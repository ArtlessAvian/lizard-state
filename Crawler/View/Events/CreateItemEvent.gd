extends Resource

var item_scene = preload("res://Crawler/View/Item.tscn")

func _init(view, event : Dictionary, roles : Array):
	var entity = event["args"]

	var item = item_scene.instance();
	item.position = view.TILESIZE * Vector2(entity.positionX, entity.positionY);
	
	view.find_node("Items").add_child(item);