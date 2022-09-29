extends Resource

var view
var event
var previous_event
var roles
var message_log
var handler_scripts
var done = true

var _subhandlers = []


func init2(vieww, eventt, previous_eventt):
	self.view = vieww
	self.event = eventt
	self.previous_event = previous_eventt
	roles = view.roles
	message_log = null
	handler_scripts = view.handlerScripts


# Returns whether this handler can run with all passed handlers
# Assumed false unless overridden
func can_run_concurrently_with(handlers: Array) -> bool:
	return false


# Called /once/ when no other events running or can run concurrently.
# If you want to do funky timing things, yield or wait for signals.
func run():
	pass
	# But it usually looks like this.

	# done = false
	# run_all_subevents_except(exceptions)
	# do timing stuff.
	# run_all_subevents_of_action(actions)

	# done = true


func run_all_subevents_except(exceptions: Array):
	for subevent in event.subevents:
		if subevent.action in exceptions:
			continue
		var subhandler = handler_scripts[subevent.action].new()
		subhandler.run()
		_subhandlers.append(subhandler)


func run_all_subevents_of_type(exceptions: Array):
	for subevent in event.subevents:
		if not subevent.action in exceptions:
			continue
		var subhandler = handler_scripts[subevent.action].new()
		subhandler.run()
		_subhandlers.append(subhandler)


# Don't mess with this.
func is_done():
	if not done:
		return false
	for subhandler in _subhandlers:
		if not subhandler.is_done():
			return false
	return true
