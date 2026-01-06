using System;
using Godot;
using Raele.GodotUtils.Adapters;
using Raele.GodotUtils.Extensions;

namespace Raele.GodotUtils;

/// <summary>
/// Interface for activity nodes. An activity is a node that can be active or inactive, has the methods Start() and
/// Finish(), that others can call to request it to starts or stop, and performs some processing while active. It also
/// has events that are emitted when the activity is starting or finishing.
/// </summary>
public interface IActivity : INode
{
	//==================================================================================================================
	#region STATICS
	//==================================================================================================================


	//==================================================================================================================
	#endregion
	//==================================================================================================================
	#region ABSTRACTS
	//==================================================================================================================

	public Node.ProcessModeEnum ProcessModeWhenActive { get; set; }
	public Node.ProcessModeEnum ProcessModeWhenInactive { get; set; }

	public bool IsActive { get; protected set; }
	public TimeSpan ActiveTimeSpan { get; protected set; }

	public event Action<string, Variant, GodotCancellationController> EventWillStart;
	public event Action<string, Variant> EventStarted;
	public event Action<string, Variant, GodotCancellationController> EventWillFinish;
	public event Action<string, Variant> EventFinished;

	protected void EmitSignalWillStart(string mode, Variant argument, GodotCancellationController controller);
	protected void EmitSignalStarted(string mode, Variant argument);
	protected void EmitSignalWillFinish(string reason, Variant details, GodotCancellationController controller);
	protected void EmitSignalFinished(string reason, Variant details);

	//==================================================================================================================
	#endregion
	//==================================================================================================================
	#region CONCRETES
	//==================================================================================================================

	/// <summary>
	/// Requests the activity to start. Returns true if the activity did start, or false if the request was rejected.
	/// </summary>
	public bool Start(string mode = "", Variant argument = new Variant())
	{
		GodotCancellationController controller = new();
		this.EmitSignalWillStart(mode, argument, controller);
		if (controller.IsCancellationRequested)
			return false;
		this.AsNode().ProcessMode = this.ProcessModeWhenActive;
		this.ActiveTimeSpan = TimeSpan.Zero;
		this.IsActive = true;
		Engine.GetSceneTree().PhysicsFrame += this.OnPhysicsFrame;
		this.EmitSignalStarted(mode, argument);
		return true;
	}

	/// <summary>
	/// Requests the activity to finish. Returns true if the activity did finish, or false if the request was rejected.
	/// </summary>
	public bool Finish(string reason = "", Variant details = new Variant())
	{
		GodotCancellationController controller = new();
		this.EmitSignalWillFinish(reason, details, controller);
		if (controller.IsCancellationRequested)
			return false;
		this.AsNode().ProcessMode = this.ProcessModeWhenInactive;
		this.ActiveTimeSpan = TimeSpan.Zero;
		this.IsActive = false;
		this.EmitSignalFinished(reason, details);
		return true;
	}

	private void OnPhysicsFrame()
	{
		if (this.IsActive)
			this.ActiveTimeSpan += TimeSpan.FromSeconds(this.AsNode().GetPhysicsProcessDeltaTime());
		else
			Engine.GetSceneTree().PhysicsFrame -= this.OnPhysicsFrame;
	}

	//==================================================================================================================
	#endregion
	//==================================================================================================================
}

public static class IActivityExtensions
{
	public static IActivity AsActivity(this IActivity activity) => activity;
}
