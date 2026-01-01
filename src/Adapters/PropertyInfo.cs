using System.Collections.Generic;
using System.Linq;
using Godot;

namespace Raele.GodotUtils.Adapters;

public record PropertyInfo
{
	public required string Name { get; init; }
	public required string ClassName { get; init; }
	public required Variant.Type Type { get; init; }
	public required PropertyHint Hint { get; init; }
	public required string HintString { get; init; }
	public required HashSet<PropertyUsageFlags> Usage { get; init; }
	public long UsageBitmask => this.Usage.Aggregate(0L, (acc, flag) => acc | (long) flag);

	public static PropertyInfo FromDictionary(Godot.Collections.Dictionary dict)
	{
		long usageFlags = dict["usage"].AsInt64();
		return new PropertyInfo
		{
			Name = dict["name"].AsString(),
			ClassName = dict["class_name"].AsString(),
			Type = (Variant.Type) dict["type"].AsInt64(),
			Hint = (PropertyHint) dict["hint"].AsInt64(),
			HintString = dict["hint_string"].AsString(),
			Usage = Enumerable.Range(0, 64)
				.Select(bit => 1L << bit)
				.Where(mask => (usageFlags & mask) != 0)
				.Select(mask => (PropertyUsageFlags) mask)
				.ToHashSet(),
		};
	}
}
