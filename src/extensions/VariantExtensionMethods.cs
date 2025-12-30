using System;
using System.Linq;
using Godot;

namespace Raele.GodotUtils.Extensions;

public static class VariantExtensionMethods
{
	extension (Variant self)
	{
		public Variant As(Variant.Type type)
			=> type switch
			{
				Variant.Type.Nil => new Variant(),
				Variant.Type.Bool => self.AsBool(),
				Variant.Type.Int => self.AsInt64(),
				Variant.Type.Float => self.AsSingle(),
				Variant.Type.String => self.AsString(),
				Variant.Type.Vector2 => self.AsVector2(),
				Variant.Type.Vector2I => self.AsVector2I(),
				Variant.Type.Rect2 => self.AsRect2(),
				Variant.Type.Rect2I => self.AsRect2I(),
				Variant.Type.Vector3 => self.AsVector3(),
				Variant.Type.Vector3I => self.AsVector3I(),
				Variant.Type.Transform2D => self.AsTransform2D(),
				Variant.Type.Vector4 => self.AsVector4(),
				Variant.Type.Vector4I => self.AsVector4I(),
				Variant.Type.Plane => self.AsPlane(),
				Variant.Type.Quaternion => self.AsQuaternion(),
				Variant.Type.Aabb => self.AsAabb(),
				Variant.Type.Basis => self.AsBasis(),
				Variant.Type.Transform3D => self.AsTransform3D(),
				Variant.Type.Projection => self.AsProjection(),
				Variant.Type.Color => self.AsColor(),
				Variant.Type.StringName => self.AsStringName(),
				Variant.Type.NodePath => self.AsNodePath(),
				Variant.Type.Rid => self.AsRid(),
				Variant.Type.Object => self.AsGodotObject(),
				Variant.Type.Callable => self.AsCallable(),
				Variant.Type.Signal => self.AsSignal(),
				Variant.Type.Dictionary => self.AsGodotDictionary(),
				Variant.Type.Array => self.AsGodotArray(),
				Variant.Type.PackedByteArray => self.AsByteArray(),
				Variant.Type.PackedInt32Array => self.AsInt32Array(),
				Variant.Type.PackedInt64Array => self.AsInt64Array(),
				Variant.Type.PackedFloat32Array => self.AsFloat32Array(),
				Variant.Type.PackedFloat64Array => self.AsFloat64Array(),
				Variant.Type.PackedStringArray => self.AsStringArray(),
				Variant.Type.PackedVector2Array => self.AsVector2Array(),
				Variant.Type.PackedVector3Array => self.AsVector3Array(),
				Variant.Type.PackedColorArray => self.AsColorArray(),
				Variant.Type.PackedVector4Array => self.AsVector4Array(),
				_ => self
			};

		public Variant Validated(Variant.Type type, PropertyHint hint, string hintString)
		{
			Variant input = self.As(type);
			switch (hint)
			{
				case PropertyHint.Range: {
					if (type != Variant.Type.Float)
					{
						return input;
					}
					string[] parts = hintString.Split(",").Select(str => str.Trim()).ToArray();
					float rangeMin = parts.Contains("or_less")
						? float.NegativeInfinity
						: parts.ElementAtOrDefault(0) is string minStr && !string.IsNullOrWhiteSpace(minStr)
							? float.Parse(minStr)
							: float.NegativeInfinity;
					float rangeMax = parts.Contains("or_greater")
						? float.PositiveInfinity
						: parts.ElementAtOrDefault(1) is string maxStr && !string.IsNullOrWhiteSpace(maxStr)
							? float.Parse(maxStr)
							: float.PositiveInfinity;
					float clamped = Math.Clamp(input.AsSingle(), rangeMin, rangeMax);
					float step = parts.ElementAtOrDefault(2) is string stepStr && !string.IsNullOrWhiteSpace(stepStr)
						? float.Parse(stepStr)
						: 0f;
					float rounded = Mathf.IsZeroApprox(step) ? clamped : Mathf.Round(clamped / step) * step;
					return rounded;
				}
				case PropertyHint.Enum:
					switch (type)
					{
						case Variant.Type.Int: {
							long[] options = hintString.Split(",")
								.Select(str => long.Parse(str.Split(":").Last()))
								.ToArray();
							return options.Contains(input.AsInt64()) ? input : default(long);
						}
						case Variant.Type.String: {
							string[] options = hintString.Split(",").Select(str => str.Split(":").First().Trim()).ToArray();
							return options.Contains(input.AsString()) ? input : "";
						}
						default:
							return input;
					}
				case PropertyHint.ColorNoAlpha:
					return input.AsColor() with { A = 1f };
				case PropertyHint.TypeString:
					return input; // TODO
				case PropertyHint.ArrayType:
					return input; // TODO
				case PropertyHint.DictionaryType:
					return input; // TODO
				case PropertyHint.LocaleId:
					return input; // TODO
				case PropertyHint.LocalizableString:
					return input; // TODO
				case PropertyHint.NodeType:
					if (type != Variant.Type.Object)
						return input;
					return hintString.Split(",").Select(s => s.Trim()).Contains(input.AsGodotObject().GetType().Name)
						? input
						: new Variant();
				case PropertyHint.InputName:
					if (type != Variant.Type.String)
						return input;
					return InputMap.GetActions().Contains(input.AsString()) ? input : "";
				default:
					return input;
			};
		}
	}
}
