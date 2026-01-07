using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using Godot.Collections;
using Raele.GodotUtils.Adapters;
using Raele.GodotUtils.Extensions;

namespace Raele.GodotUtils;

/// <summary>
/// Interface for activity nodes. An activity is a node that can be active or inactive, and performs some processing
/// while active.
///
/// To start the activity, call the <see cref="Start"/> method. To stop it, call the <see cref="Finish"/> method. The
/// virtual methods <see cref="_WillStart"/>, <see cref="_Started"/>, <see cref="_WillFinish"/>, and
/// <see cref="_Finished"/> can be overridden to hook into the activity's lifecycle. Additionally activity also emits
/// events when starting and finishing, that others can subscribe to.
///
/// The activity automatically changes its process mode based on its state and the <see cref="ProcessModeWhenActive"/>
/// and <see cref="ProcessModeWhenInactive"/> properties. You can check whether the activity is active and for how long
/// using the <see cref="IsActive"/> and <see cref="ActiveTimeSpan"/> properties.
/// </summary>
public interface IActivity : INode
{
	//==================================================================================================================
	#region STATICS
	//==================================================================================================================

	//==================================================================================================================
	#endregion
	//==================================================================================================================
	#region LOCAL TYPES
	//==================================================================================================================

	public class ActivityData
	{
		public bool IsActive = false;
		public TimeSpan ActiveTimeSpan = TimeSpan.Zero;
		public Node.ProcessModeEnum ProcessModeWhenActive = Node.ProcessModeEnum.Inherit;
		public Node.ProcessModeEnum ProcessModeWhenInactive = Node.ProcessModeEnum.Disabled;
	}

	//==================================================================================================================
	#endregion
	//==================================================================================================================
	#region ABSTRACTS
	//==================================================================================================================

	public ActivityData InstanceFields { get; }

	public event Action<string, Variant, GodotCancellationController> EventWillStart;
	public event Action<string, Variant> EventStarted;
	public event Action<string, Variant, GodotCancellationController> EventWillFinish;
	public event Action<string, Variant> EventFinished;

	public void InvokeEventWillStart(string mode, Variant argument, GodotCancellationController controller);
	public void InvokeEventStarted(string mode, Variant argument);
	public void InvokeEventWillFinish(string reason, Variant details, GodotCancellationController controller);
	public void InvokeEventFinished(string reason, Variant details);

	protected virtual void _WillStart(string mode, Variant argument, GodotCancellationController controller) {}
	protected virtual void _Started(string mode, Variant argument) {}
	protected virtual void _ProcessActive(double delta) {}
	protected virtual void _PhysicsProcessActive(double delta) {}
	protected virtual void _WillFinish(string reason, Variant details, GodotCancellationController controller) {}
	protected virtual void _Finished(string reason, Variant details) {}

	//==================================================================================================================
	#endregion
	//==================================================================================================================
	#region CONCRETES
	//==================================================================================================================

	public bool IsActive
	{
		get => this.InstanceFields.IsActive;
		private set => this.InstanceFields.IsActive = value;
	}
	public TimeSpan ActiveTimeSpan
	{
		get => this.InstanceFields.ActiveTimeSpan;
		private set => this.InstanceFields.ActiveTimeSpan = value;
	}
	public Node.ProcessModeEnum ProcessModeWhenActive
	{
		get => this.InstanceFields.ProcessModeWhenActive;
		set => this.InstanceFields.ProcessModeWhenActive = value;
	}
	public Node.ProcessModeEnum ProcessModeWhenInactive
	{
		get => this.InstanceFields.ProcessModeWhenInactive;
		set => this.InstanceFields.ProcessModeWhenInactive = value;
	}

	public Array<Dictionary> HandleGetPropertyList()
		=> new List<GodotPropertyInfo>()
			.Append(new()
			{
				Name = nameof(IActivity),
				Usage = [PropertyUsageFlags.Category]
			})
			.Append(new()
			{
				Name = nameof(ProcessModeWhenActive),
				Type = Variant.Type.Int,
				Hint = PropertyHint.Enum,
				HintString = Enum.GetNames<Node.ProcessModeEnum>().Join(","),
			})
			.Append(new()
			{
				Name = nameof(ProcessModeWhenInactive),
				Type = Variant.Type.Int,
				Hint = PropertyHint.Enum,
				HintString = Enum.GetNames<Node.ProcessModeEnum>().Join(","),
			})
			.Select(GodotPropertyInfo.ToGodotDictionary)
			.ToGodotArrayT();

	public void HandleValidateProperty(Godot.Collections.Dictionary property) {}

	public Variant HandleGet(StringName property)
		=> property.ToString() switch
		{
			nameof(ProcessModeWhenActive) => (long) this.ProcessModeWhenActive,
			nameof(ProcessModeWhenInactive) => (long) this.ProcessModeWhenInactive,
			_ => new Variant(),
		};
	public bool HandleSet(StringName property, Variant value)
	{
		switch (property.ToString())
		{
			case nameof(ProcessModeWhenActive):
				this.ProcessModeWhenActive = (Node.ProcessModeEnum) value.AsInt64();
				return true;
			case nameof(ProcessModeWhenInactive):
				this.ProcessModeWhenInactive = (Node.ProcessModeEnum) value.AsInt64();
				return true;
		}
		return false;
	}

	public void HandleReady() {}
	public void HandleEnterTree() {}
	public void HandleExitTree() {}

	public void HandleProcess(double delta)
	{
		if (Engine.IsEditorHint())
		{
			this.SetProcess(false);
			return;
		}
		if (this.IsActive)
			this._ProcessActive(delta);
	}

	public void HandlePhysicsProcess(double delta)
	{
		if (Engine.IsEditorHint())
		{
			this.SetPhysicsProcess(false);
			return;
		}
		if (!this.IsActive)
			return;
		this.ActiveTimeSpan += TimeSpan.FromSeconds(delta);
		this._PhysicsProcessActive(delta);
	}

	//------------------------------------------------------------------------------------------------------------------

	/// <summary>
	/// Requests the activity to start. Returns true if the activity did start, or false if the request was rejected.
	/// </summary>
	public bool Start(string mode = "", Variant argument = new Variant())
	{
		GodotCancellationController controller = new();
		this._WillStart(mode, argument, controller);
		this.InvokeEventWillStart(mode, argument, controller);
		if (controller.IsCancellationRequested)
			return false;
		this.AsNode().ProcessMode = this.ProcessModeWhenActive;
		this.ActiveTimeSpan = TimeSpan.Zero;
		this.IsActive = true;
		Callable.From(() =>
		{
			this._Started(mode, argument);
			this.InvokeEventStarted(mode, argument);
		}).CallDeferred();
		return true;
	}

	/// <summary>
	/// Requests the activity to finish. Returns true if the activity did finish, or false if the request was rejected.
	/// </summary>
	public bool Finish(string reason = "", Variant details = new Variant())
	{
		GodotCancellationController controller = new();
		this._WillFinish(reason, details, controller);
		this.InvokeEventWillFinish(reason, details, controller);
		if (controller.IsCancellationRequested)
			return false;
		this.AsNode().ProcessMode = this.ProcessModeWhenInactive;
		this.ActiveTimeSpan = TimeSpan.Zero;
		this.IsActive = false;
		Callable.From(() =>
		{
			this._Finished(reason, details);
			this.InvokeEventFinished(reason, details);
		}).CallDeferred();
		return true;
	}

	//==================================================================================================================
	#endregion
	//==================================================================================================================
}

public static class IActivityExtensions
{
	public static IActivity AsActivity(this IActivity activity) => activity;
}
