using System.Collections.Generic;
using System.Linq;
using Godot;

namespace Raele.GodotUtils.Adapters;

public record ConnectedSignal
{
	public required string Signal { get; init; }
	public required Callable Callable { get; init; }
	public required HashSet<GodotObject.ConnectFlags> Flags { get; init; }
	public long FlagsBitmask => this.Flags.Aggregate(0L, (acc, flag) => acc | (long) flag);

	public static ConnectedSignal FromDictionary(Godot.Collections.Dictionary dict)
	{
		long flags = dict["flags"].AsInt64();
		return new ConnectedSignal
		{
			Signal = dict["signal"].AsString(),
			Callable = dict["callable"].AsCallable(),
			Flags = Enumerable.Range(0, 64)
				.Select(bit => 1L << bit)
				.Where(mask => (flags & mask) != 0)
				.Select(mask => (GodotObject.ConnectFlags) mask)
				.ToHashSet()
		};
	}
}
