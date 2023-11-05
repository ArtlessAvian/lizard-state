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


# Deprecated.
# Returns whether this handler can run with all passed handlers
# Assumed false unless overridden
func can_run_concurrently_with(handlers: Array) -> bool:
	return false


# Child should check for parentability, rather than parent should check for childability.
# Assumed false unless overridden.
func can_accept_child(child: Reference) -> bool:
	return false


# Prefer `can_accept_child` when checking events in the same codebase.
# Use this only for external code linking back in. (Like for modding I guess?) 
# Assumed false unless overriden.
func can_be_child(parent: Reference) -> bool:
	return false


# Called /once/ when no other events running or can run concurrently.
# If you want to do funky timing things, set done false and yield for signals.
# Remember to set done when done.
func run():
	pass


# poll for done-ness.
func is_done():
	return not view.AnyActorAnimating()


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
