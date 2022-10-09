extends Reference

var view
var roles: Dictionary
var message_log

var event: Dictionary


func init2(vieww, eventt):
	self.view = vieww
	roles = view.roles
	message_log = view.get_node("%MessageLog")

	self.event = eventt


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


func add_message(string, color = null):
	message_log.AddMessage(format_message(string, color))


func format_message(string, color = null):
	var out = string.format(get_formatting())
	if color is String:
		out = "[color=" + color + "]" + out + "[/color]"
	return out


func get_formatting() -> Dictionary:
	var out = Dictionary()
	if event.has("subject") and roles.has(event["subject"]):
		out["subject"] = roles[event["subject"]].displayName
	if event.has("object") and roles.has(event["object"]):
		out["object"] = roles[event["object"]].displayName
	return out


func now():
	return OS.get_system_time_msecs()
