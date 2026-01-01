using System.Collections.Generic;
using System.Linq;
using Godot;

namespace Raele.GodotUtils.Adapters;

public record MethodInfo
{
	public required string Name { get; init; }
	public required PropertyInfo[] Params { get; init; }
	public required Variant[] DefaultArgs { get; init; }
	public required HashSet<MethodFlags> Flags { get; init; }
	public long FlagsBitmask => this.Flags.Aggregate(0L, (acc, flag) => acc | (long) flag);
	public required long Id { get; init; }
	public required PropertyInfo Return { get; init; }

	public static MethodInfo FromDictionary(Godot.Collections.Dictionary dict)
	{
		long flags = dict["flags"].AsInt64();
		return new MethodInfo
		{
			Name = dict["name"].AsString(),
			Params = dict["args"].AsGodotArray()
				.Select(arg => arg.AsGodotDictionary())
				.Select(PropertyInfo.FromDictionary)
				.ToArray(),
			DefaultArgs = dict["default_args"].AsGodotArray().ToArray(),
			Flags = Enumerable.Range(0, 64)
				.Select(bit => 1L << bit)
				.Where(mask => (flags & mask) != 0)
				.Select(mask => (MethodFlags) mask)
				.ToHashSet(),
			Id = dict["id"].AsInt64(),
			Return = PropertyInfo.FromDictionary(dict["return"].AsGodotDictionary()),
		};
	}
}
