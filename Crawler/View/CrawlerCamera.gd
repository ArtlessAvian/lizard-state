extends Camera2D

var focus : Node2D = self

func _process(delta):
	self.position = focus.position
	self.force_update_scroll()
