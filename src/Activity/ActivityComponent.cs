using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using Raele.GodotUtils.Adapters;
using Raele.GodotUtils.Extensions;

namespace Raele.GodotUtils;

public abstract partial class ActivityComponent : Activity, IActivityComponent
{
	//==================================================================================================================
	#region STATICS
	//==================================================================================================================

	public const string PROPERTY_GROUP_START_STRATEGY_NAME = "Start Strategy";
	public const string PROPERTY_GROUP_START_STRATEGY_PREFIX = "Start";
	public const string PROPERTY_GROUP_FINISH_STRATEGY_NAME = "Finish Strategy";
	public const string PROPERTY_GROUP_FINISH_STRATEGY_PREFIX = "Finish";

	//==================================================================================================================
	#endregion
	//==================================================================================================================
	#region EXPORTS
	//==================================================================================================================

	[Export] public bool Enabled = true;
	[Export] public TimingStrategyEnum StartStrategy
		{ get; set { field = value; this.StartStrategyImpl = this.CreateTimingStrategyHandler(value); } }
		= TimingStrategyEnum.Immediate;
	[Export] public TimingStrategyEnum FinishStrategy
		{ get; set { field = value; this.FinishStrategyImpl = this.CreateTimingStrategyHandler(value); } }
		= TimingStrategyEnum.Never;

	//==================================================================================================================
	#endregion
	//==================================================================================================================
	#region FIELDS
	//==================================================================================================================

	public StateEnum State { get; private set; } = StateEnum.Inactive;

	private ITimingStrategy StartStrategyImpl = new NeverTimingStrategy();
	private ITimingStrategy FinishStrategyImpl = new NeverTimingStrategy();

	//==================================================================================================================
	#endregion
	//==================================================================================================================
	#region COMPUTED PROPERTIES
	//==================================================================================================================

	public IActivity? ParentActivity => this.GetAncestorOrDefault<IActivity>();

	//==================================================================================================================
	#endregion
	//==================================================================================================================
	#region EVENTS & SIGNALS
	//==================================================================================================================

	//==================================================================================================================
	#endregion
	//==================================================================================================================
	#region INTERNAL TYPES
	//==================================================================================================================

	public enum StateEnum : byte
	{
		/// <summary>
		/// The owner ability is not active.
		/// </summary>
		Inactive = 0,
		/// <summary>
		/// The ability has started but this ability component has not started itself yet.
		/// </summary>
		StandBy = 64,
		/// <summary>
		/// The ability component is active.
		/// </summary>
		Started = 128,
		/// <summary>
		/// The ability component has finished its activity and is now waiting for the owner ability to finish before it
		/// can be activated again.
		/// </summary>
		Finished = 192,
	}

	public enum TimingStrategyEnum : byte
	{
		Immediate = 0,
		AfterDuration = 64,
		AnimationMarker = 128,
		WhenExpressionIsTrue = 192,
		Never = 255,
	}

	//==================================================================================================================
	#endregion
	//==================================================================================================================
	#region OVERRIDES & VIRTUALS
	//==================================================================================================================

	public override Godot.Collections.Array<Godot.Collections.Dictionary> _GetPropertyList()
		=> Enumerable.Empty<GodotPropertyInfo>()
			.Append(new()
			{
				Name = PROPERTY_GROUP_START_STRATEGY_NAME,
				Usage = [PropertyUsageFlags.Group],
				HintString = PROPERTY_GROUP_START_STRATEGY_PREFIX,
			})
			.Concat(
				this.StartStrategyImpl._GetPropertyList()
					.Select(GodotPropertyInfo.FromGodotDictionary)
					.Select(property => new GodotPropertyInfo(property)
					{
						Name = PROPERTY_GROUP_START_STRATEGY_PREFIX + property.Name,
					})
			)
			.Append(new GodotPropertyInfo()
			{
				Name = PROPERTY_GROUP_FINISH_STRATEGY_NAME,
				Usage = [PropertyUsageFlags.Group],
				HintString = PROPERTY_GROUP_FINISH_STRATEGY_PREFIX,
			})
			.Concat(
				this.FinishStrategyImpl._GetPropertyList()
					.Select(GodotPropertyInfo.FromGodotDictionary)
					.Select(property => new GodotPropertyInfo(property)
					{
						Name = PROPERTY_GROUP_FINISH_STRATEGY_PREFIX + property.Name,
					})
			)
			.Select(GodotPropertyInfo.ToGodotDictionary)
			.Concat(base._GetPropertyList() ?? [])
			.ToGodotArrayT();
	public override Variant _Get(StringName property)
		=> property.ToString() switch
		{
			string propertyName when propertyName.StartsWith(PROPERTY_GROUP_START_STRATEGY_PREFIX)
				=> this.StartStrategyImpl._Get(property.ToString().Substring(PROPERTY_GROUP_START_STRATEGY_PREFIX.Length)),
			string propertyName when propertyName.StartsWith(PROPERTY_GROUP_FINISH_STRATEGY_PREFIX)
				=> this.FinishStrategyImpl._Get(property.ToString().Substring(PROPERTY_GROUP_FINISH_STRATEGY_PREFIX.Length)),
			_ => base._Get(property),
		};
	public override bool _Set(StringName property, Variant value)
	{
		if (property.ToString().StartsWith(PROPERTY_GROUP_START_STRATEGY_PREFIX))
			return this.StartStrategyImpl._Set(property.ToString().Substring(PROPERTY_GROUP_START_STRATEGY_PREFIX.Length), value);
		if (property.ToString().StartsWith(PROPERTY_GROUP_FINISH_STRATEGY_PREFIX))
			return this.FinishStrategyImpl._Set(property.ToString().Substring(PROPERTY_GROUP_FINISH_STRATEGY_PREFIX.Length), value);
		return base._Set(property, value);
	}
	public override bool _PropertyCanRevert(StringName property)
		=> property.ToString() switch
		{
			string propertyName when propertyName.StartsWith(PROPERTY_GROUP_START_STRATEGY_PREFIX)
				=> this.StartStrategyImpl._PropertyCanRevert(property.ToString().Substring(PROPERTY_GROUP_START_STRATEGY_PREFIX.Length)),
			string propertyName when propertyName.StartsWith(PROPERTY_GROUP_FINISH_STRATEGY_PREFIX)
				=> this.FinishStrategyImpl._PropertyCanRevert(property.ToString().Substring(PROPERTY_GROUP_FINISH_STRATEGY_PREFIX.Length)),
			_ => base._PropertyCanRevert(property),
		};
	public override Variant _PropertyGetRevert(StringName property)
		=> property.ToString() switch
		{
			string propertyName when propertyName.StartsWith(PROPERTY_GROUP_START_STRATEGY_PREFIX)
				=> this.StartStrategyImpl._PropertyGetRevert(property.ToString().Substring(PROPERTY_GROUP_START_STRATEGY_PREFIX.Length)),
			string propertyName when propertyName.StartsWith(PROPERTY_GROUP_FINISH_STRATEGY_PREFIX)
				=> this.FinishStrategyImpl._PropertyGetRevert(property.ToString().Substring(PROPERTY_GROUP_FINISH_STRATEGY_PREFIX.Length)),
			_ => base._PropertyGetRevert(property),
		};
	public override void _EnterTree()
	{
		base._EnterTree();
		if (Engine.IsEditorHint())
			return;
		this.Started += this.OnStarted;
		this.Finished += this.OnFinished;
		this.ParentActivity?.EventWillStart += this._ParentActivityWillStart;
		this.ParentActivity?.EventStarted += this._ParentActivityStarted;
		this.ParentActivity?.EventWillFinish += this._ParentActivityWillFinish;
		this.ParentActivity?.EventFinished += this._ParentActivityFinished;
	}
	public override void _ExitTree()
	{
		base._ExitTree();
		if (Engine.IsEditorHint())
			return;
		this.Started -= this.OnStarted;
		this.Finished -= this.OnFinished;
		this.ParentActivity?.EventWillStart -= this._ParentActivityWillStart;
		this.ParentActivity?.EventStarted -= this._ParentActivityStarted;
		this.ParentActivity?.EventWillFinish -= this._ParentActivityWillFinish;
		this.ParentActivity?.EventFinished -= this._ParentActivityFinished;
	}
	public override void _PhysicsProcess(double delta)
	{
		base._PhysicsProcess(delta);
		if (Engine.IsEditorHint())
			return;
		if (this.State == StateEnum.StandBy && this.TestStartConditions())
			this.Start();
		if (this.State == StateEnum.Started && this.TestFinishConditions())
			this.Finish();
	}

	protected virtual void _ParentActivityWillStart(string mode, Variant argument, GodotCancellationController controller) {}
	protected virtual void _ParentActivityStarted(string mode, Variant argument) {}
	protected virtual void _ParentActivityWillFinish(string reason, Variant details, GodotCancellationController controller) {}
	protected virtual void _ParentActivityFinished(string reason, Variant details) {}

	//==================================================================================================================
	#endregion
	//==================================================================================================================
	#region METHODS
	//==================================================================================================================

	private void OnStarted(string mode, Variant argument)
		=> this.State = StateEnum.Started;
	private void OnFinished(string reason, Variant details)
		=> this.State = StateEnum.Finished;

	private bool TestStartConditions() => this.StartStrategyImpl.Test();
	private bool TestFinishConditions() => this.FinishStrategyImpl.Test();

	private ITimingStrategy CreateTimingStrategyHandler(TimingStrategyEnum strategy)
		=> strategy switch
		{
			TimingStrategyEnum.Immediate => new ImmediateTimingStrategy(),
			TimingStrategyEnum.AfterDuration => new AfterDurationTimingStrategy(this),
			TimingStrategyEnum.AnimationMarker => new AnimationMarkerTimingStrategy(),
			TimingStrategyEnum.WhenExpressionIsTrue => new WhenExpressionIsTrueTimingStrategy(this),
			TimingStrategyEnum.Never => new NeverTimingStrategy(),
			_ => throw new NotImplementedException($"Timing strategy {strategy:G} ({strategy:D}) is not implemented yet."),
		};

	//==================================================================================================================
		#endregion
	//==================================================================================================================
		#region Strategy Implementations
	//==================================================================================================================

	private interface ITimingStrategy
	{
		public virtual Godot.Collections.Array<Godot.Collections.Dictionary> _GetPropertyList() => [];
		public virtual Variant _Get(StringName property) => new Variant();
		public virtual bool _Set(StringName property, Variant value) => false;
		public virtual bool _PropertyCanRevert(StringName property) => false;
		public virtual Variant _PropertyGetRevert(StringName property) => new Variant();
		public abstract bool Test();
	}

	private partial class ImmediateTimingStrategy : ITimingStrategy
	{
		bool ITimingStrategy.Test() => true;
	}

	private partial class AfterDurationTimingStrategy(ActivityComponent WRAPPER) : ITimingStrategy
	{
		private const double DEFAULT_DURATION = 1d;
		private double Duration = DEFAULT_DURATION;
		private double ElapsedSeconds;
		Godot.Collections.Array<Godot.Collections.Dictionary> ITimingStrategy._GetPropertyList()
			=> new List<GodotPropertyInfo>()
			{
				new()
				{
					Name = nameof(Duration),
					Type = Variant.Type.Float,
					HintString = "suffix:s",
					Usage = [PropertyUsageFlags.Default],
					DefaultValue = DEFAULT_DURATION,
				},
			}
			.Select(GodotPropertyInfo.ToGodotDictionary)
			.ToGodotArrayT();
		Variant ITimingStrategy._Get(StringName property)
			=> property.ToString() == nameof(Duration) ? this.Duration : Variant.NULL;
		bool ITimingStrategy._Set(StringName property, Variant value)
		{
			switch (property.ToString())
			{
				case nameof(Duration):
					this.Duration = value.AsDouble();
					return true;
			}
			return false;
		}
		bool ITimingStrategy._PropertyCanRevert(StringName property)
			=> property.ToString() == nameof(Duration) && this.Duration != DEFAULT_DURATION;
		Variant ITimingStrategy._PropertyGetRevert(StringName property)
			=> property.ToString() == nameof(Duration) ? DEFAULT_DURATION : Variant.NULL;
		bool ITimingStrategy.Test()
		{
			this.ElapsedSeconds += WRAPPER.GetProcessDeltaTime();
			if (this.ElapsedSeconds >= this.Duration)
			{
				this.ElapsedSeconds = 0.0;
				return true;
			}
			return false;
		}
	}

	private partial class AnimationMarkerTimingStrategy : ITimingStrategy
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
		Godot.Collections.Array<Godot.Collections.Dictionary> ITimingStrategy._GetPropertyList()
			=> new GodotPropertyInfo[]
				{
					new()
					{
						Name = nameof(AnimationPlayer),
						Type = Variant.Type.Object,
						Hint = PropertyHint.NodeType,
						HintString = nameof(Godot.AnimationPlayer),
						Usage = [PropertyUsageFlags.Default, PropertyUsageFlags.UpdateAllIfModified],
						DefaultValue = this._PropertyGetRevert(nameof(AnimationPlayer)),
					},
					this.AnimationPlayer == null
						? new()
						{
							Name = nameof(Animation),
							Type = Variant.Type.String,
							DefaultValue = this._PropertyGetRevert(nameof(Animation)),
						}
						: new()
						{
							Name = nameof(Animation),
							Type = Variant.Type.String,
							Hint = PropertyHint.Enum,
							HintString = this.AnimationPlayer.GetAnimationList().Append("").JoinIntoString(","),
							Usage = [PropertyUsageFlags.Default, PropertyUsageFlags.UpdateAllIfModified],
							DefaultValue = this._PropertyGetRevert(nameof(Animation)),
						},
					this.AnimationObject is not Animation animation
						? new()
						{
							Name = nameof(Marker),
							Type = Variant.Type.String,
							DefaultValue = this._PropertyGetRevert(nameof(Marker)),
						}
						: new()
						{
							Name = nameof(Marker),
							Type = Variant.Type.String,
							Hint = PropertyHint.Enum,
							HintString = animation.GetMarkerNames().Append("").JoinIntoString(","),
							DefaultValue = this._PropertyGetRevert(nameof(Marker)),
						}
				}
				.Select(GodotPropertyInfo.ToGodotDictionary)
				.ToGodotArrayT();
		Variant ITimingStrategy._Get(StringName property)
			=> property.ToString() switch
			{
				nameof(AnimationPlayer) => this.AnimationPlayer ?? Variant.NULL,
				nameof(Animation) => this.Animation,
				nameof(Marker) => this.Marker,
				_ => Variant.NULL,
			};
		bool ITimingStrategy._Set(StringName property, Variant value)
		{
			switch (property.ToString())
			{
				case nameof(AnimationPlayer):
					this.AnimationPlayer = value.AsGodotObject<AnimationPlayer>();
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
		bool ITimingStrategy._PropertyCanRevert(StringName property) => this._PropertyCanRevert(property);
		private bool _PropertyCanRevert(StringName property)
			=> property.ToString() switch
			{
				nameof(AnimationPlayer) => this.AnimationPlayer != this._PropertyGetRevert(nameof(AnimationPlayer)).AsGodotObject(),
				nameof(Animation) => this.Animation != this._PropertyGetRevert(nameof(Animation)).AsString(),
				nameof(Marker) => this.Marker != this._PropertyGetRevert(nameof(Marker)).AsString(),
				_ => false,
			};
		Variant ITimingStrategy._PropertyGetRevert(StringName property) => this._PropertyGetRevert(property);
		private Variant _PropertyGetRevert(StringName property)
			=> property.ToString() switch
			{
				nameof(AnimationPlayer) => Variant.NULL,
				nameof(Animation) => "",
				nameof(Marker) => "",
				_ => Variant.NULL,
			};
		bool ITimingStrategy.Test()
			=> (this.AnimationPlayer?.CurrentAnimationPosition ?? 0d)
					>= this.MarkerTime - Mathf.Epsilon;
	}

	private partial class WhenExpressionIsTrueTimingStrategy(ActivityComponent WRAPPER) : ITimingStrategy
	{
		private Node? Context = WRAPPER;
		private Variant Param = new Variant();
		private string Expression
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
		Godot.Collections.Array<Godot.Collections.Dictionary> ITimingStrategy._GetPropertyList()
			=> new GodotPropertyInfo[]
			{
				new()
				{
					Name = nameof(Context),
					Type = Variant.Type.Object,
					Hint = PropertyHint.NodeType,
					DefaultValue = this._PropertyGetRevert(nameof(Context)),
				},
				new()
				{
					Name = nameof(Param),
					Type = Variant.Type.Nil,
					Usage = [PropertyUsageFlags.Default, PropertyUsageFlags.NilIsVariant],
					DefaultValue = this._PropertyGetRevert(nameof(Param)),
				},
				new()
				{
					Name = nameof(Expression),
					Type = Variant.Type.String,
					Hint = PropertyHint.Expression,
					DefaultValue = this._PropertyGetRevert(nameof(Expression)),
				},
			}
			.Select(GodotPropertyInfo.ToGodotDictionary)
			.ToGodotArrayT();
		Variant ITimingStrategy._Get(StringName property)
			=> property.ToString() switch
			{
				nameof(Context) => this.Context ?? Variant.NULL,
				nameof(Param) => this.Param,
				nameof(Expression) => this.Expression,
				_ => new Variant(),
			};
		bool ITimingStrategy._Set(StringName property, Variant value)
		{
			switch (property.ToString())
			{
				case nameof(Context):
					this.Context = value.AsGodotObject<Node>();
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
		bool ITimingStrategy._PropertyCanRevert(StringName property) => this._PropertyCanRevert(property);
		private bool _PropertyCanRevert(StringName property)
			=> property.ToString() switch
			{
				nameof(Context) => this.Context != this._PropertyGetRevert(nameof(Context)).AsGodotObject(),
				nameof(Param) => !this.Param.Equals(this._PropertyGetRevert(nameof(Param))),
				nameof(Expression) => this.Expression != this._PropertyGetRevert(nameof(Expression)).AsString(),
				_ => false,
			};
		Variant ITimingStrategy._PropertyGetRevert(StringName property) => this._PropertyGetRevert(property);
		private Variant _PropertyGetRevert(StringName property)
			=> property.ToString() switch
			{
				nameof(Context) => WRAPPER,
				nameof(Param) => Variant.NULL,
				nameof(Expression) => "",
				_ => Variant.NULL,
			};
		bool ITimingStrategy.Test()
			=> this.Interpreter.Execute([this.Param], this.Context).AsBool();
	}

	private partial class NeverTimingStrategy : ITimingStrategy
	{
		bool ITimingStrategy.Test() => false;
	}

	//==================================================================================================================
	#endregion
	//==================================================================================================================
}
