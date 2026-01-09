using System.Linq;
using Godot;
using Raele.GodotUtils.Adapters;
using Raele.GodotUtils.Extensions;

namespace Raele.GodotUtils.ActivitySystem.TimingStrategies;

[Tool][GlobalClass]
public partial class AnimationMarkerTimingStrategy : TimingStrategy
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
	public override Godot.Collections.Array<Godot.Collections.Dictionary> _GetPropertyList()
		=> new GodotPropertyInfo[]
			{
				new()
				{
					Name = nameof(AnimationPlayer),
					Type = Variant.Type.Object,
					Hint = PropertyHint.NodeType,
					HintString = nameof(Godot.AnimationPlayer),
					Usage = [PropertyUsageFlags.Default, PropertyUsageFlags.UpdateAllIfModified],
				},
				this.AnimationPlayer == null
					? new()
					{
						Name = nameof(Animation),
						Type = Variant.Type.String,
					}
					: new()
					{
						Name = nameof(Animation),
						Type = Variant.Type.String,
						Hint = PropertyHint.Enum,
						HintString = this.AnimationPlayer.GetAnimationList().Append("").JoinIntoString(","),
						Usage = [PropertyUsageFlags.Default, PropertyUsageFlags.UpdateAllIfModified],
					},
				this.AnimationObject is not Animation animation
					? new()
					{
						Name = nameof(Marker),
						Type = Variant.Type.String,
					}
					: new()
					{
						Name = nameof(Marker),
						Type = Variant.Type.String,
						Hint = PropertyHint.Enum,
						HintString = animation.GetMarkerNames().Append("").JoinIntoString(","),
					}
			}
			.Select(GodotPropertyInfo.ToGodotDictionary)
			.ToGodotArrayT();
	public override Variant _Get(StringName property)
		=> property.ToString() switch
		{
			nameof(AnimationPlayer) => Variant.From(this.AnimationPlayer),
			nameof(Animation) => this.Animation,
			nameof(Marker) => this.Marker,
			_ => Variant.NULL,
		};
	public override bool _Set(StringName property, Variant value)
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
	public override bool _PropertyCanRevert(StringName property)
		=> property.ToString() switch
		{
			nameof(AnimationPlayer) => this.AnimationPlayer != this._PropertyGetRevert(nameof(AnimationPlayer)).AsGodotObject(),
			nameof(Animation) => this.Animation != this._PropertyGetRevert(nameof(Animation)).AsString(),
			nameof(Marker) => this.Marker != this._PropertyGetRevert(nameof(Marker)).AsString(),
			_ => false,
		};
	public override Variant _PropertyGetRevert(StringName property)
		=> property.ToString() switch
		{
			nameof(AnimationPlayer) => Variant.NULL,
			nameof(Animation) => "",
			nameof(Marker) => "",
			_ => Variant.NULL,
		};
	public override bool Test(IActivity? activity)
		=> (this.AnimationPlayer?.CurrentAnimationPosition ?? 0d)
				>= this.MarkerTime - Mathf.Epsilon;
}
