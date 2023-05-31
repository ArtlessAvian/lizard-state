using Godot;

public static class DeepCopyHelper
{
    private static bool IsGodotValue(object thing)
    {
        return !(thing is Godot.Object || thing is Godot.Collections.Array || thing is Godot.Collections.Dictionary);
    }

    // Alternatively, IsResourceNotCopyable.
    // Some resources are like values/constants but can still be copied for shenanigans.
    private static bool IsResourceValueLike(Resource resource)
    {
        if (resource is Script) { return true; }
        // if (resource is Species) { return true; }
        // if (resource is ItemData) { return true; }
        return false;
    }

    public static object DeepCopy(object variant)
    {
        return DeepCopyInternal(variant, new System.Collections.Generic.Dictionary<object, object>());
    }

    private static object DeepCopyInternal(object variant, System.Collections.Generic.Dictionary<object, object> copies)
    {
        if (IsGodotValue(variant)) { return variant; }
        if (variant is Resource res && IsResourceValueLike(res)) { return variant; }

        // If we know this reference, return our copy.
        if (copies.ContainsKey(variant)) { return copies[variant]; }

        // Remember a copy in `copies`, then replace everything recursively.
        if (variant is Godot.Collections.Array array)
        {
            Godot.Collections.Array copy = new Godot.Collections.Array();
            copies[variant] = copy;

            foreach (object x in array)
            {
                copy.Add(DeepCopyInternal(x, copies));
            }
            return copy;
        }
        else if (variant is Godot.Collections.Dictionary dict)
        {
            Godot.Collections.Dictionary copy = new Godot.Collections.Dictionary();
            copies[variant] = copy;

            foreach (object key in dict.Keys)
            {
                copy[DeepCopyInternal(key, copies)] = DeepCopyInternal(dict[key], copies);
            }
            return copy;
        }
        else if (variant is Resource resource)
        {
            Resource copy = resource.Duplicate();
            copies[variant] = copy;

            foreach (Godot.Collections.Dictionary property in resource.GetPropertyList())
            {
                // Don't copy non-exported stuff. (Temporaries, or C# conveniences).
                if (((int)property["usage"] & (int)PropertyUsageFlags.Storage) == 0)
                {
                    continue;
                }
                // Since we shallow copied, we can just skip over things without rewriting them.
                Variant.Type type = (Variant.Type)property["type"];
                if (type != Variant.Type.Object && type != Variant.Type.Array && type != Variant.Type.Dictionary)
                {
                    continue;
                }
                string name = (string)property["name"];
                copy.Set(name, DeepCopyInternal(resource.Get(name), copies));
            }

            return copy;
        }

        // Some sort of cursed Godot.Object.
        GD.PrintErr(variant.GetType());
        return variant;
    }
}