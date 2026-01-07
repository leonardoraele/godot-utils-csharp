using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Godot;
using Raele.GodotUtils.Adapters;
using Raele.GodotUtils.Extensions;

namespace Raele.GodotUtils;

/// <summary>
/// Interface for activity components. Activity components are nodes that are descendants of an <see cref="IActivity"/>
/// node, and can hook into the activity's lifecycle events to perform some processing. It does so by subscribing to the
/// activity's events.
///
/// It is also an activity in itself, meaning other components can hook into its lifecycle as well, recursively. It
/// provides timing strategies to control how its own activity starts and finishes in relation to the parent activity's
/// lifecycle, allowing for flexible behavior composition.
/// </summary>
public interface IActivityComponent : INode, IActivity
{
	//==================================================================================================================
		#region STATICS
	//==================================================================================================================

	private static readonly ConditionalWeakTable<IActivityComponent, ITimingStrategyHandler> StartStrategyHandlerTable
		= new();
	private static readonly ConditionalWeakTable<IActivityComponent, ITimingStrategyHandler> FinishStrategyHandlerTable
		= new();

	//==================================================================================================================
		#endregion
	//==================================================================================================================
		#region ABSTRACTS & VIRTUALS
	//==================================================================================================================

	public bool Enabled { get; set; }
	public TimingStrategyEnum StartStrategy { get; set; }
	public TimingStrategyEnum FinishStrategy { get; set; }
	public StateEnum State { get; protected set; }

	public virtual void _ActivityWillStart(string mode, Variant argument, GodotCancellationController controller) {}
	public virtual void _ActivityStarted(string mode, Variant argument) {}
	public virtual void _ActivityWillFinish(string reason, Variant details, GodotCancellationController controller) {}
	public virtual void _ActivityFinished(string reason, Variant details) {}

	//==================================================================================================================
		#endregion
	//==================================================================================================================
		#region GODOT EVENT HANDLERS
	//==================================================================================================================

	public Godot.Collections.Array<Godot.Collections.Dictionary> HandleGetPropertyList()
		=> new List<GodotPropertyInfo>()
			.Append(new()
			{
				Name = nameof(IActivityComponent),
				Usage = [PropertyUsageFlags.Category],
			})
			.Append(new()
			{
				Name = nameof(Enabled),
				Type = Variant.Type.Bool,
				DefaultValue = true,
			})
			.Append(new()
			{
				Name = "Start Strategy",
				Usage = [PropertyUsageFlags.Group],
			})
			.Append(new()
			{
				Name = nameof(StartStrategy),
				Type = Variant.Type.Int,
				Hint = PropertyHint.Enum,
				HintString = Enum.GetNames<TimingStrategyEnum>().Join(","),
				Usage = [PropertyUsageFlags.Default, PropertyUsageFlags.UpdateAllIfModified],
				DefaultValue = (long) TimingStrategyEnum.Immediate,
			})
			.Concat(this.StartStrategyHandler.GetExports())
			.Append(new()
			{
				Name = "Finish Strategy",
				Usage = [PropertyUsageFlags.Group],
			})
			.Append(new()
			{
				Name = nameof(FinishStrategy),
				Type = Variant.Type.Int,
				Hint = PropertyHint.Enum,
				HintString = Enum.GetNames<TimingStrategyEnum>().Join(","),
				Usage = [PropertyUsageFlags.Default, PropertyUsageFlags.UpdateAllIfModified],
				DefaultValue = (long) TimingStrategyEnum.Never,
			})
			.Concat(this.FinishStrategyHandler.GetExports())
			.Select(GodotPropertyInfo.ToGodotDictionary)
			.ToGodotArrayT();

	public void HandleValidateProperty(Godot.Collections.Dictionary property)
	{
		// TODO
	}

	public bool HandleSetProperty(string propertyName, Variant value)
	{
		if (propertyName == nameof(StartStrategy))
			this.StartStrategyHandler = CreateTimingStrategyHandler(this.StartStrategy);
		else if (propertyName == nameof(FinishStrategy))
			this.FinishStrategyHandler = CreateTimingStrategyHandler(this.FinishStrategy);
		return this.StartStrategyHandler.HandleSetProperty(propertyName, value)
			|| this.FinishStrategyHandler.HandleSetProperty(propertyName, value);
	}

	public Variant HandleGetProperty(string propertyName)
		=> this.StartStrategyHandler.HandleGetProperty(propertyName)
			?? this.FinishStrategyHandler.HandleGetProperty(propertyName)
			?? new Variant();

	public void HandleEnterTree()
	{
		this.EventStarted += this.OnStarted;
		this.EventFinished += this.OnFinished;
		this.Activity?.EventWillStart += this.OnActivityWillStart;
		this.Activity?.EventStarted += this.OnActivityStarted;
		this.Activity?.EventWillFinish += this.OnActivityWillFinish;
		this.Activity?.EventFinished += this.OnActivityFinished;
	}

	public void HandleExitTree()
	{
		this.EventStarted -= this.OnStarted;
		this.EventFinished -= this.OnFinished;
		this.Activity?.EventWillStart -= this.OnActivityWillStart;
		this.Activity?.EventStarted -= this.OnActivityStarted;
		this.Activity?.EventWillFinish -= this.OnActivityWillFinish;
		this.Activity?.EventFinished -= this.OnActivityFinished;
	}

	public new void HandleProcess(double delta)
		=> this.AsActivity().HandleProcess(delta);

	public new void HandlePhysicsProcess(double delta)
	{
		if (this.State == StateEnum.StandBy && this.TestStartConditions())
			this.Start();
		if (this.State == StateEnum.Started && this.TestFinishConditions())
			this.Finish();
		this.AsActivity().HandlePhysicsProcess(delta);
	}

	//------------------------------------------------------------------------------------------------------------------

	private void OnStarted(string mode, Variant argument)
		=> this.State = StateEnum.Started;
	private void OnFinished(string reason, Variant details)
		=> this.State = StateEnum.Finished;

	private void OnActivityWillStart(string mode, Variant argument, GodotCancellationController controller)
		=> this._ActivityWillStart(mode, argument, controller);
	private void OnActivityStarted(string mode, Variant argument)
	{
		this.State = StateEnum.StandBy;
		this._ActivityStarted(mode, argument);
	}
	private void OnActivityWillFinish(string reason, Variant details, GodotCancellationController controller)
		=> this._ActivityWillFinish(reason, details, controller);
	private void OnActivityFinished(string reason, Variant details)
	{
		this.State = StateEnum.Finished;
		this._ActivityFinished(reason, details);
	}

	//==================================================================================================================
		#endregion
	//==================================================================================================================
		#region OTHER CONCRETES
	//==================================================================================================================

	public IActivity? Activity => this.GetParentOrNull<IActivity>();

	private bool TestStartConditions() => this.StartStrategyHandler.Test(this);
	private bool TestFinishConditions() => this.FinishStrategyHandler.Test(this);

	private ITimingStrategyHandler StartStrategyHandler
	{
		get => StartStrategyHandlerTable.GetOrAdd(this, _ => CreateTimingStrategyHandler(this.StartStrategy));
		set => StartStrategyHandlerTable.AddOrUpdate(this, value);
	}
	private ITimingStrategyHandler FinishStrategyHandler
	{
		get => FinishStrategyHandlerTable.GetOrAdd(this, _ => CreateTimingStrategyHandler(this.FinishStrategy));
		set => FinishStrategyHandlerTable.AddOrUpdate(this, value);
	}

	private static ITimingStrategyHandler CreateTimingStrategyHandler(TimingStrategyEnum strategy)
		=> strategy switch
		{
			TimingStrategyEnum.Immediate => new ImmediateTimingStrategyHandler(),
			TimingStrategyEnum.AfterDuration => new FixedTimeTimingStrategyHandler(),
			TimingStrategyEnum.AnimationMarker => new AnimationMarkerTimingStrategyHandler(),
			TimingStrategyEnum.WhenExpressionIsTrue => new ExpressionTimingStrategyHandler(),
			TimingStrategyEnum.Never => new NeverTimingStrategyHandler(),
			_ => throw new NotImplementedException($"Timing strategy {strategy} is not implemented yet."),
		};

	//==================================================================================================================
		#endregion
	//==================================================================================================================
		#region LOCAL TYPES
	//==================================================================================================================

	public enum StateEnum : byte
	{
		/// <summary>
		/// The owner ability is not active.
		/// </summary>
		Inactive,
		/// <summary>
		/// The ability has started but this ability component has not started itself yet.
		/// </summary>
		StandBy,
		/// <summary>
		/// The ability component is active.
		/// </summary>
		Started,
		/// <summary>
		/// The ability component has finished its activity and is now waiting for the owner ability to finish before it
		/// can be activated again.
		/// </summary>
		Finished,
	}

	public enum TimingStrategyEnum : byte
	{
		Immediate,
		AfterDuration,
		AnimationMarker,
		WhenExpressionIsTrue,
		Never,
	}

	//==================================================================================================================
		#endregion
	//==================================================================================================================
		#region Strategy Implementations
	//==================================================================================================================

	private interface ITimingStrategyHandler
	{
		public GodotPropertyInfo[] GetExports();
		public GodotPropertyInfo HandleValidateProperty(GodotPropertyInfo property);
		public Variant HandleGetProperty(string propertyName);
		public bool HandleSetProperty(string propertyName, Variant value);
		public bool Test(IActivityComponent component);
	}

	private class ImmediateTimingStrategyHandler : ITimingStrategyHandler
	{
		public GodotPropertyInfo[] GetExports() => [];
		public GodotPropertyInfo HandleValidateProperty(GodotPropertyInfo property) => property;
		public Variant HandleGetProperty(string propertyName) => new Variant();
		public bool HandleSetProperty(string propertyName, Variant value) => false;
		public bool Test(IActivityComponent component) => true;
	}

	private class FixedTimeTimingStrategyHandler : ITimingStrategyHandler
	{
		public double Duration { get; set; } = 1.0;
		private double elapsedSeconds = 0.0;

		public GodotPropertyInfo[] GetExports() => [new()
		{
			Name = nameof(Duration),
			Type = Variant.Type.Float,
			HintString = "suffix:s",
			Usage = [PropertyUsageFlags.Default],
			DefaultValue = 1d,
		}];
		public GodotPropertyInfo HandleValidateProperty(GodotPropertyInfo property) => property;
		public Variant HandleGetProperty(string propertyName) => propertyName switch
		{
			nameof(Duration) => this.Duration,
			_ => new Variant(),
		};
		public bool HandleSetProperty(string propertyName, Variant value)
		{
			if (propertyName == nameof(Duration))
			{
				this.Duration = value.AsDouble();
				return true;
			}
			return false;
		}
		public bool Test(IActivityComponent component)
		{
			this.elapsedSeconds += component.AsNode().GetProcessDeltaTime();
			if (this.elapsedSeconds >= this.Duration)
			{
				this.elapsedSeconds = 0.0;
				return true;
			}
			return false;
		}
	}

	private class AnimationMarkerTimingStrategyHandler : ITimingStrategyHandler
	{
		private AnimationPlayer? AnimationPlayer;
		private string Animation = "";
		private string Marker = "";
		private Animation? AnimationObject
		{
			get
			{
				if (this.AnimationPlayer == null)
					return null;
				string animationName = string.IsNullOrWhiteSpace(this.Animation)
					? this.AnimationPlayer.CurrentAnimation
					: this.Animation;
				if (string.IsNullOrWhiteSpace(animationName))
					return null;
				return this.AnimationPlayer.GetAnimation(animationName);
			}
		}
		private double MarkerTime
			=> this.AnimationObject is Animation animation
				&& animation.HasMarker(this.Marker)
					? animation.GetMarkerTime(this.Marker)
					: double.PositiveInfinity;
		public GodotPropertyInfo[] GetExports() => [
			new()
			{
				Name = nameof(AnimationPlayer),
				Type = Variant.Type.Object,
				Hint = PropertyHint.NodeType,
				HintString = nameof(Godot.AnimationPlayer),
				Usage = [PropertyUsageFlags.Default, PropertyUsageFlags.UpdateAllIfModified],
			},
			new()
			{
				Name = nameof(Animation),
				Type = Variant.Type.String,
			},
			new()
			{
				Name = nameof(Marker),
				Type = Variant.Type.String,
			},
		];
		public GodotPropertyInfo HandleValidateProperty(GodotPropertyInfo property)
		{
			switch (property.Name)
			{
				case nameof(this.Animation):
					if (this.AnimationPlayer == null)
						return property;
					return new(property)
					{
						Name = property.Name,
						Type = Variant.Type.String,
						Hint = PropertyHint.Enum,
						HintString = this.AnimationPlayer.GetAnimationList().Join(","),
						Usage = [PropertyUsageFlags.Default, PropertyUsageFlags.UpdateAllIfModified],
					};
				case nameof(this.Marker):
					if (this.AnimationObject is not Animation animation)
						return property;
					return new(property)
					{
						Name = property.Name,
						Type = Variant.Type.String,
						Hint = PropertyHint.Enum,
						HintString = animation.GetMarkerNames().Join(","),
					};
			}
			return property;
		}
		public Variant HandleGetProperty(string propertyName) => propertyName switch
		{
			nameof(AnimationPlayer) => this.AnimationPlayer ?? new Variant(),
			nameof(Animation) => this.Animation,
			nameof(Marker) => this.Marker,
			_ => new Variant(),
		};
		public bool HandleSetProperty(string propertyName, Variant value)
		{
			switch (propertyName)
			{
				case nameof(AnimationPlayer):
					this.AnimationPlayer = value.AsGodotObject() as AnimationPlayer;
					return true;
				case nameof(Animation):
					this.Animation = value.AsString();
					return true;
				case nameof(Marker):
					this.Marker = value.AsString();
					return true;
			}
			return false;
		}
		public bool Test(IActivityComponent component)
			=> (this.AnimationPlayer?.CurrentAnimationPosition ?? 0d)
					>= this.MarkerTime - Mathf.Epsilon;
	}

	private class ExpressionTimingStrategyHandler : ITimingStrategyHandler
	{
		public Node? Context;
		public Variant Param;
		public string Expression
			{ get; set { field = value; this.Interpreter = null!; } }
			= "";
		private Expression Interpreter
		{
			get
			{
				if (field == null)
				{
					field = new();
					field.Parse(this.Expression, ["param"]);
				}
				return field;
			}
			set;
		}

		public GodotPropertyInfo[] GetExports() => [
			new()
			{
				Name = nameof(Context),
				Type = Variant.Type.Object,
				Hint = PropertyHint.NodeType,
			},
			new()
			{
				Name = nameof(Param),
				Type = Variant.Type.Nil,
				Usage = [PropertyUsageFlags.Default, PropertyUsageFlags.NilIsVariant],
			},
			new()
			{
				Name = nameof(Expression),
				Type = Variant.Type.String,
				DefaultValue = "",
			},
		];
		public GodotPropertyInfo HandleValidateProperty(GodotPropertyInfo property) => property;
		public Variant HandleGetProperty(string propertyName) => propertyName switch
		{
			nameof(Context) => this.Context ?? new Variant(),
			nameof(Param) => this.Param,
			nameof(Expression) => this.Expression,
			_ => new Variant(),
		};
		public bool HandleSetProperty(string propertyName, Variant value)
		{
			switch (propertyName)
			{
				case nameof(Context):
					this.Context = value.AsGodotObject() as Node;
					return true;
				case nameof(Param):
					this.Param = value;
					return true;
				case nameof(Expression):
					this.Expression = value.AsString();
					return true;
			}
			return false;
		}
		public bool Test(IActivityComponent component)
			=> this.Interpreter.Execute([this.Param], this.Context).AsBool();
	}

	private class NeverTimingStrategyHandler : ITimingStrategyHandler
	{
		public GodotPropertyInfo[] GetExports() => [];
		public GodotPropertyInfo HandleValidateProperty(GodotPropertyInfo property) => property;
		public Variant HandleGetProperty(string propertyName) => new Variant();
		public bool HandleSetProperty(string propertyName, Variant value) => false;
		public bool Test(IActivityComponent component) => false;
	}

	//==================================================================================================================
		#endregion
	//==================================================================================================================
}

public static class IActivityComponentExtensions
{
	public static IActivityComponent AsActivityComponent(this IActivityComponent component) => component;
}
