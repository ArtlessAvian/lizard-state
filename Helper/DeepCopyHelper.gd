extends Node


# Allows early exiting.
func _is_value_type(type):
	return not (type == TYPE_OBJECT or type == TYPE_ARRAY or type == TYPE_DICTIONARY)


func _is_uncopyable(resource: Resource):
	# This behaves poorly. The entire point why we aren't doing [Resource].Duplicate(true).
	if resource.is_class("Script"):
		return true

	# Just kind of large. Not a big deal. Used by the EditorGenerator.
	if resource.is_class("PackedScene"):
		printerr("TODO: Avoid PackedScene in Save.")
		return true

	return false


func _deep_copy_internal(variant, copies: Dictionary):
	if _is_value_type(typeof(variant)):
		return variant
	if variant == null:
		return variant

	if copies.has(variant):
		return copies[variant]

	# Create a new or shallow copy, remember in copies, *then* recur.
	match typeof(variant):
		TYPE_ARRAY:
			var copy = Array()
			copies[variant] = copy
			for el in variant:
				copy.append(_deep_copy_internal(el, copies))
			return copy

		TYPE_DICTIONARY:
			var copy = Dictionary()
			copies[variant] = copy
			for key in variant:
				copy[_deep_copy_internal(key, copies)] = _deep_copy_internal(variant[key], copies)
			return copy

		TYPE_OBJECT:
			if not variant is Resource:
				printerr("deep_copy called on non-resource type ", variant)
				return variant
			if _is_uncopyable(variant):
				return variant

			var copy = variant.duplicate()
			copies[variant] = copy
			for property in variant.get_property_list():
				if property["usage"] & PROPERTY_USAGE_STORAGE == 0:
					continue
				# Since we shallow-copied, we can skip values.
				if _is_value_type(property["type"]):
					continue
				var name = property["name"]
				copy.set(name, _deep_copy_internal(copy.get(name), copies))
			return copy

	# Unreachable.
	printerr("Deepcopy reached uncovered case!")
	return variant


func deep_copy(variant):
	return _deep_copy_internal(variant, Dictionary())
