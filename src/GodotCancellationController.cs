using Godot;

namespace Raele.GodotUtils;

public partial class GodotCancellationController : GodotObject
{
	public CancellationToken Token { get; private set; } = new();
	public bool IsCancellationRequested { get; private set; } = false;
	public Variant Reason { get; private set; } = new Variant();
	public void Abort(Variant reason)
	{
		this.Token.EmitSignal(CancellationToken.SignalName.Aborted, reason);
		this.IsCancellationRequested = true;
		this.Reason = reason;
	}
}
