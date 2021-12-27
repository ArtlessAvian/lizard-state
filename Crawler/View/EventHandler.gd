extends Resource

var view
var event
var previous_event
var roles
var message_log

# not really used, since this object is pooled per view.
func _init():
    pass

# stores a bunch of local variables, so that the extenders have easy access.
# do not override, please.
func reinit(vieww, eventt, previous_eventt):
    self.view = vieww
    self.event = eventt
    self.previous_event = previous_eventt
    roles = view.roles
    message_log = null