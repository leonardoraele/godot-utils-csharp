using System.Threading.Tasks;
using Godot;

namespace Raele.GodotUtils;

public partial class CancellationToken : GodotObject
{
	[Signal] public delegate void AbortedEventHandler(Variant reason);
	public Task<Variant> OnceAborted()
	{
		TaskCompletionSource<Variant> cts = new();
		this.Connect(
			SignalName.Aborted,
			Callable.From((Variant reason) => cts.SetResult(reason)),
			(uint) ConnectFlags.OneShot
		);
		return cts.Task;
	}
}
