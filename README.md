
Source generator for Godot that allows using an equivalent to GDScript's `@onready` in C#. By decorating fields with the `[GetNode]` attribute, it automatically initializes them when the scene is loaded. This simplifies C# code for node initialization in Godot projects.

Features:
- `[GetNode]`, `[GetRes]` attributes - for initializing properties in the _Ready method
- `[OnReady]` attribute - marks method that will be run after all of the variables marked with `[GetNode]` or `[GetRes]` are initialized
