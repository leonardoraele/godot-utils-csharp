using System.Collections.Generic;
using System.Linq;
using Godot;
using Raele.GodotUtils.Extensions;

namespace Raele.GodotUtils.Adapters;

public record GodotPropertyInfo
{
	//==================================================================================================================
	// STATICS
	//==================================================================================================================

	public static Godot.Collections.Dictionary ToGodotDictionary(GodotPropertyInfo subject)
		=> subject.BackingDict.Duplicate();
	public static GodotPropertyInfo FromGodotDictionary(Godot.Collections.Dictionary dict)
		=> new(dict);
	public static Godot.Collections.Array<Godot.Collections.Dictionary> BuildPropertyList(GodotPropertyInfo[] properties)
		=> properties.Select(ToGodotDictionary).ToGodotArrayT();

	public GodotPropertyInfo()
		=> this.BackingDict = new();
	public GodotPropertyInfo(Godot.Collections.Dictionary dict)
		=> this.BackingDict = dict.Duplicate();
	public GodotPropertyInfo(GodotPropertyInfo other)
		=> this.BackingDict = other.BackingDict.Duplicate();

	//==================================================================================================================
	// FIELDS
	//==================================================================================================================

	private Godot.Collections.Dictionary BackingDict;

	//==================================================================================================================
	// COMPUTED PROPERTIES
	//==================================================================================================================

	public string Name
	{
		get => BackingDict["name"].AsString();
		init => BackingDict["name"] = value;
	}
	public string ClassName
	{
		get => BackingDict.GetValueOrDefault("class_name", "").AsString();
		init => BackingDict["class_name"] = value;
	}
	public Variant.Type Type
	{
		get => (Variant.Type) BackingDict.GetValueOrDefault("type", (long) Variant.Type.Nil).AsInt64();
		init => BackingDict["type"] = (long) value;
	}
	public PropertyHint Hint
	{
		get => (PropertyHint) BackingDict.GetValueOrDefault("hint", (long) PropertyHint.None).AsInt64();
		init => BackingDict["hint"] = (long) value;
	}
	public string HintString
	{
		get => BackingDict.GetValueOrDefault("hint_string", "").AsString();
		init => BackingDict["hint_string"] = value;
	}
	public long UsageBitmask
	{
		get => BackingDict.GetValueOrDefault("usage", (long) PropertyUsageFlags.Default).AsInt64();
		init => BackingDict["usage"] = value;
	}
	public HashSet<PropertyUsageFlags> Usage
	{
		get => Enumerable.Range(0, 64)
			.Select(bit => 1L << bit)
			.Where(mask => (this.UsageBitmask & mask) != 0)
			.Select(mask => (PropertyUsageFlags) mask)
			.ToHashSet();
		init => this.UsageBitmask = value.Aggregate(0L, (acc, flag) => acc | (long) flag);
	}
	public Variant DefaultValue
	{
		get => BackingDict.GetValueOrDefault("default_value", Variant.NULL);
		init => BackingDict["default_value"] = value;
	}

	//==================================================================================================================
	// METHODS
	//==================================================================================================================

	public Godot.Collections.Dictionary ToGodotDictionary() => ToGodotDictionary(this);
}
