﻿//HintName: Sword.g.cs
// <auto-generated />
#nullable disable

using Godot;
using GodotAutoOnReady.SourceGenerators.Attributes;

public partial class Sword : Node
{
	private Sword()
	{
		Ready += pdoXhzeD10wZA80OjI8u_OnReady;
	}

	private void pdoXhzeD10wZA80OjI8u_OnReady()
	{
		_Ready();
	}

	public override void _Ready()
	{
		Node = GetNode<DummyNode>("%SomeProp");
		Field = GetNode<DummyNode>("%SomeField");
	}
}
