extends "../EventHandler.gd"

func should_wait_after():
	return view.AnyActorAnimating()

	## This doesn't work, since the model is /far/ ahead
	## The model only stops for user inputs, which is when
	## it is safe to sync.
	# if view.AnyActorAnimating():		
	#	return true
	# view.ModelSync()
	# return false
