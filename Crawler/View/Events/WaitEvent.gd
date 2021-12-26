extends Resource

var view

func _init():
	pass

func run(_view, event : Dictionary, roles : Array):
	view = _view

func can_consume():
	if not view.AnyActorAnimating():		
		## This doesn't work, since the model is /far/ ahead
		## The model only stops for user inputs, which is when
		## it is safe to sync.
		# view.ModelSync()
		return true
	return false
