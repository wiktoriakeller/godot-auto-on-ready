﻿//HintName: Sword.g.cs
// <auto-generated />
#nullable disable

using Godot;
using GodotAutoOnReady.Attributes;

namespace RPGGame
{
	public partial class Sword : Node
	{
		private Sword()
		{
			Ready += e5c9d7030bada2fe792ad7e4a26ca44379a73f3fe16805ac1359e46759a1571a_OnReady;
		}

		private void e5c9d7030bada2fe792ad7e4a26ca44379a73f3fe16805ac1359e46759a1571a_OnReady()
		{
			_Ready();
		}

		public override void _Ready()
		{
			base._Ready();
			Node = GetNode<DummyNode>("%SomeProp");
			Field = GetNode<DummyNode>("%SomeField");
			_field2 = GetNode<DummyNode>("Field2");
			_mobTimer = GetNode<DummyNode>("MobTimer");
			_c = GetNode<DummyNode>("C");
			_ = GetNode<DummyNode>("_");
			c = GetNode<DummyNode>("C");
		}
	}
}
