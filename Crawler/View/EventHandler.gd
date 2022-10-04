extends Resource

var view
var event
var previous_event
var roles
var message_log


func init2(vieww, eventt, previous_eventt):
	self.view = vieww
	self.event = eventt
	self.previous_event = previous_eventt
	roles = view.roles
	message_log = null


# Returns whether this handler can run with all passed handlers
# Assumed false unless overridden
func can_run_concurrently_with(handlers: Array) -> bool:
	return false


# Called /once/ when no other events running or can run concurrently.
# If you want to do funky timing things, set done false and yield for signals.
# Remember to set done when done.
func run():
	pass


# poll for done-ness.
func is_done():
	return not view.AnyActorAnimating()


# used so other handlers can *other* handlers can run currently with you.
# (compare to can_run_concurrently_with, which you decide which others you can run with.)
#
func get_importance():
	return 999


## Helpers
func is_same_subject():
	return event.subject == previous_event.subject


func now():
	return OS.get_system_time_msecs()
