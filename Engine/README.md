# Engine
This directory contains logic for the dungeon crawl.

Models contain Entities. Entities take turns doing Actions.
Actions cause the Model to emit a sequence of events.
Between Actions, Systems run for consistency.

There are interfaces for Actions and Systems. There should not be any implementations here, unless necessary.

This should only be written in C#. The Godot SceneTree should not be referenced.

## Why Godot Dependency
Godot is used to serialize and deserialize the game state.
To do this, we make everything in the game state a resource.
This handles cycles and shared references, which is convenient (but not necessary. Currently the game has shared references.)

The engine lives off the scene tree. Scenes and Nodes may be loaded as a way to allow in engine data editing, eg maps stored in TileMaps.
Remember to free after use.

## Why not Godot?
With the Godot dependency, the game is pinned to Godot 3. This isn't a *big* problem. 
The most obvious place to move to is Godot 4. We can also have it not rely on anything, letting anything use this as a library. 

Making everything a resource has small downsides. Its manageable, acknowledging the following list of complaints.

### Construction
Creating new objects extending resource requires we load it as a CSharpScript, then building it from the Script. The building is not statically typed, so any changes could cause later runtime crashes. 

(An safe option to get around this is to create static constructors that load their class as CSharpScripts and construct themselves.)

### Interfaces
Godot will not serialize anything unless it knows it is a Resource.

In the Model, CrawlerSystems were stored in a List\<Resource\> to allow serialization.
We could not enforce that everything in the list implements CrawlerSystem.
(We have replaced the interface with an abstract class. They weren't abstract since one system had inherited.)

Actions are abstract classes extending Resource. Godot can serialize them.

### Non-Godot Value Types
We cannot (easily) create value types without also making them resources.
Doing so invites a lot of overhead.

We can just have mutable types and/or eat the cost. EntityState is (currently an enum, but is planned to be) an object representing a state.
We will be switching states very often, entire states or making mutations.

Alternatively, we can internally store serializable primitives, and use a property to work with them at a higher level.

Godot can serialize their own Vector2, though they're a (float, float) pair.
This game is in a grid, so (int, int) pairs would be *better*. The game has little float math (except probability and angles).

Currently, we have a Vector2i type. We cannot store it, but we can expose it as a property, and internally export two ints.

### Non-Godot Collections
Godot can serialize Lists and Dictionaries in System.Collections.Generic, given the generics are also serializable.
The restriction is a bit awkward to work around.

SparseMatrix is essentially a dictionary from (int, int) to int.
The first can be expressed in Godot as a Vector2 (float, float), so internally the serialized dictionary uses Vector2 as a key.
Another alternative is dictionary from int to dictionaries from int to int.

### Deepcopying CSharpScript?
The default deepcopy "copies" CSharpScript resources!
This makes some sense for GDScript, ignoring security. The Script contains the code, which works.
Resources as data classes means the scripts attached to them act sort of like a schema and API?
Possible errors come from a user calling a deserialized resource expecting the API of the script on disk and getting the deserialized script.

My custom deepcopy leaves CSharpScript references as external references.
The classes may change, and deserialized resources will just use default values for missing values.

Default copying CSharpScript is kind of the worst of both worlds, creating a resource that does nothing.
There's a GitHub issue about it already, I'm sure. And selective deepcopy is in Godot 4 IIRC.