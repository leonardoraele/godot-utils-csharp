using System;
using System.Threading;
using System.Threading.Tasks;
using Godot;

namespace Raele.MyProject;

/// <summary>
/// Interface for processors that operate over discrete time intervals. For nodes that perform certain processsing over
/// a defined period of time, allowing for start and finish events, as well as tracking active time.
/// </summary>
public interface IDiscreteIntervalProcessor
{
	public bool IsActive { get; }
	public TimeSpan ActiveTimeSpan { get; }

	public event Action<Variant, CancellationTokenSource> WillStartEvent;
	public event Action<Variant> StartedEvent;
	/// <summary>
	/// Only emitted when Finish() is called, not when the processor stops by itself, e.g. if its max process time has
	/// been reached.
	/// </summary>
	public event Action<Variant, CancellationTokenSource> FinishRequestedEvent;
	public event Action<Variant> FinishedEvent;

	/// <summary>
	/// Completes the task after all WillStartEvent handles have been called and none have cancelled the provided
	/// CancellationTokenSource. Rejects if cancelled.
	/// </summary>
	public Task Start(Variant argument);
	/// <summary>
	/// Completes the task after all FinishRequestedEvent handles have been called and none have cancelled the provided
	/// CancellationTokenSource. Rejects if cancelled.
	/// </summary>
	public Task Finish(Variant reason);
}
