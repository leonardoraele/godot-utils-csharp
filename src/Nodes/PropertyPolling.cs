using System.Linq;
using Godot;
using Raele.GodotUtils.Extensions;

namespace Raele.GodotUtils.Nodes;

[Tool][GlobalClass][Icon($"./{nameof(PropertyPolling)}.svg")]
public partial class PropertyPolling : Node
{
	//==================================================================================================================
	// STATICS
	//==================================================================================================================

	// public static readonly string MyConstant = "";

	//==================================================================================================================
	// EXPORTS
	//==================================================================================================================

	[Export] public Node? TargetNode
		{ get => field ?? this.GetParent(); set { field = value; this.NotifyPropertyListChanged(); this.UpdateConfigurationWarnings(); } }
	[Export] public string TargetProperty
		{ get; set { field = value; this.UpdateConfigurationWarnings(); } }
		= "";
	[Export] public Node? SourceNode
		{ get; set { field = value; this.NotifyPropertyListChanged(); this.UpdateConfigurationWarnings(); } }
	[Export] public string SourceProperty
		{ get; set { field = value; this.UpdateConfigurationWarnings(); } }
		= "";

	[ExportGroup("Use Expression")]
	[Export(PropertyHint.GroupEnable)] public bool ExpressionEnabled
		{ get; set { field = value; this.NotifyPropertyListChanged(); } }
		= false;
	[Export(PropertyHint.Expression)] public string Expression = "";
	[Export] public Godot.Collections.Dictionary<string, Variant> Variables = [];

	[ExportGroup("Options")]
	[Export] public UpdateModeEnum UpdateMode = UpdateModeEnum.IdleFrames;

	[ExportGroup("Debug")]
	[Export] public bool RunInEditor = false;

	//==================================================================================================================
	// FIELDS
	//==================================================================================================================

	private Expression ExpressionInterpreter
	{
		get
		{
			if (field == null || Engine.IsEditorHint())
			{
				field = new();
				field.Parse(this.Expression, this.Variables.Keys.Append("value").ToArray());
			}
			return field;
		}
	}

	//==================================================================================================================
	// COMPUTED PROPERTIES
	//==================================================================================================================

	//==================================================================================================================
	// EVENTS & SIGNALS
	//==================================================================================================================

	// [Signal] public delegate void EventHandler();

	//==================================================================================================================
	// INTERNAL TYPES
	//==================================================================================================================

	public enum UpdateModeEnum : sbyte {
		IdleFrames = 16,
		PhysicsFrames = 32,
		Manually = 96,
	}

	//==================================================================================================================
	// OVERRIDES & VIRTUALS
	//==================================================================================================================

	public override string[] _GetConfigurationWarnings()
		=> (base._GetConfigurationWarnings() ?? [])
			.AppendIf(this.TargetNode == null, $"{nameof(TargetNode)} is null.")
			.AppendIf(string.IsNullOrWhiteSpace(this.TargetProperty), $"{nameof(TargetProperty)} is empty.")
			.AppendIf(this.SourceNode == null, $"{nameof(SourceNode)} is null.")
			.AppendIf(
				!this.ExpressionEnabled && string.IsNullOrWhiteSpace(this.SourceProperty),
				$"{nameof(SourceProperty)} is empty."
			)
			.ToArray();

	public override void _ValidateProperty(Godot.Collections.Dictionary property)
	{
		base._ValidateProperty(property);
		switch (property["name"].AsString())
		{
			case nameof(this.TargetProperty): {
				if (this.TargetNode == null)
					return;
				string options = this.TargetNode.GetPropertyList()
					.Where(prop => prop["usage"].AsPropertyUsageFlags() != PropertyUsageFlags.Group
						&& prop["usage"].AsPropertyUsageFlags() != PropertyUsageFlags.Subgroup
						&& prop["usage"].AsPropertyUsageFlags() != PropertyUsageFlags.Category
					)
					.Select(prop => prop["name"].AsString())
					.Order()
					.JoinIntoString(",");
				property["hint"] = (long) PropertyHint.EnumSuggestion;
				property["hint_string"] = options;
				break;
			}
			case nameof(this.SourceProperty): {
				if (this.ExpressionEnabled)
				{
					property["usage"] = (long) PropertyUsageFlags.None;
					return;
				}
				if (this.SourceNode == null)
					return;
				Variant.Type targetType = this.TargetNode?.GetPropertyList()
					.FirstOrDefault(prop => prop["name"].AsString() == this.TargetProperty)
					?["type"].AsVariantType()
					?? Variant.Type.Nil;
				string options = this.SourceNode.GetPropertyList()
					.Where(prop => prop["usage"].AsPropertyUsageFlags() != PropertyUsageFlags.Group
						&& prop["usage"].AsPropertyUsageFlags() != PropertyUsageFlags.Subgroup
						&& prop["usage"].AsPropertyUsageFlags() != PropertyUsageFlags.Category
					)
					.Where(prop => prop["type"].AsVariantType().IsConvertibleTo(targetType))
					.Select(prop => prop["name"].AsString())
					.Order()
					.JoinIntoString(",");
				property["hint"] = (long) PropertyHint.EnumSuggestion;
				property["hint_string"] = options;
				break;
			}
		}
	}

	public override void _Process(double delta)
	{
		base._Process(delta);
		if (this.UpdateMode == UpdateModeEnum.IdleFrames)
			this.UpdateProperty();
	}

	public override void _PhysicsProcess(double delta)
	{
		base._PhysicsProcess(delta);
		if (this.UpdateMode == UpdateModeEnum.PhysicsFrames)
			this.UpdateProperty();
	}

	//==================================================================================================================
	// METHODS
	//==================================================================================================================

	public void UpdateProperty()
	{
		if (Engine.IsEditorHint() && !this.RunInEditor)
			return;
		Variant value = this.SourceNode?.GetIndexed(this.SourceProperty) ?? new Variant();
		if (this.ExpressionEnabled)
		{
			value = this.ExpressionInterpreter.Execute(
				this.Variables.Values.Append(value).ToGodotArray(),
				this.SourceNode
			);
			if (this.ExpressionInterpreter.HasExecuteFailed())
				return;
		}
		this.TargetNode?.SetIndexed(this.TargetProperty, value);
	}
}
