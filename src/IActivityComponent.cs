using Godot;
using Raele.GodotUtils.Adapters;

namespace Raele.GodotUtils;

/// <summary>
/// Interface for activity components. Activity components are nodes that are descendants of an IActivity node, and can
/// hook into the activity's lifecycle events to perform some processing, e.g. only while the activity is active.
/// </summary>
public interface IActivityComponent : INode
{
	public IActivity? Activity => this.GetParentOrNull<IActivity>();

	protected void OnEnterTree()
	{
		this.Activity?.EventWillStart += this.OnActivityWillStart;
		this.Activity?.EventStarted += this.OnActivityStarted;
		this.Activity?.EventWillFinish += this.OnActivityWillFinish;
		this.Activity?.EventFinished += this.OnActivityFinished;
	}

	protected void OnExitTree()
	{
		this.Activity?.EventWillStart -= this.OnActivityWillStart;
		this.Activity?.EventStarted -= this.OnActivityStarted;
		this.Activity?.EventWillFinish -= this.OnActivityWillFinish;
		this.Activity?.EventFinished -= this.OnActivityFinished;
	}

	protected void OnProcess(double delta)
	{
		if (this.Activity?.IsActive != true)
			return;
		this._ActivityProcess(delta);
	}

	protected void OnPhysicsProcess(double delta)
	{
		if (this.Activity?.IsActive != true)
			return;
		this._ActivityPhysicsProcess(delta);
	}

	private void OnActivityWillStart(string mode, Variant argument, GodotCancellationController controller)
	{
		this._ActivityWillStart(mode, argument, controller);
	}
	private void OnActivityStarted(string mode, Variant argument)
	{
		this._ActivityStarted(mode, argument);
	}
	private void OnActivityWillFinish(string reason, Variant details, GodotCancellationController controller)
	{
		this._ActivityWillFinish(reason, details, controller);
	}
	private void OnActivityFinished(string reason, Variant details)
	{
		this._ActivityFinished(reason, details);
	}

	public virtual void _ActivityWillStart(string mode, Variant argument, GodotCancellationController controller) {}
	public virtual void _ActivityStarted(string mode, Variant argument) {}
	public virtual void _ActivityProcess(double delta) {}
	public virtual void _ActivityPhysicsProcess(double delta) {}
	public virtual void _ActivityWillFinish(string reason, Variant details, GodotCancellationController controller) {}
	public virtual void _ActivityFinished(string reason, Variant details) {}
}

public static class IActivityComponentExtensions
{
	public static IActivityComponent AsActivityComponent(this IActivityComponent component) => component;
}
