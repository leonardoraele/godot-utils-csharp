using Godot;
using Raele.GodotUtils.Extensions;

namespace Raele.GodotUtils.IntrospectionSystem.VariantTests;

[Tool][GlobalClass]
public partial class EqualityTest : VariantTest
{
	[Export] public VariantSource? Parameter;
	[Export] public bool Not;

	protected override bool _ReferencesSceneNode()
		=> this.Parameter?.ReferencesSceneNode() ?? false;
	protected override bool _Test(Variant variant)
		=> (this.Parameter?.GetValue() ?? Variant.NULL).Equals(variant) != this.Not;
}
