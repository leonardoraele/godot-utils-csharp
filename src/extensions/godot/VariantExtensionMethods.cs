using System;
using System.Linq;
using Godot;

namespace Raele.GodotUtils.Extensions;

public static class VariantExtensionMethods
{
	extension (Variant self)
	{
		public static Variant Nil() => new Variant();

		public Variant As(Variant.Type type)
			=> type switch
			{
				Variant.Type.Nil => new Variant(),
				Variant.Type.Bool => self.AsBool(),
				Variant.Type.Int => self.AsInt64(),
				Variant.Type.Float => self.AsDouble(),
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

		public static Variant.Type Typeof<[MustBeVariant] T>()
			=> Typeof(typeof(T));
		public static Variant.Type Typeof(Type type)
			=> type == typeof(bool) ? Variant.Type.Bool
			: type == typeof(char) ? Variant.Type.String
			: type == typeof(sbyte) ? Variant.Type.Int
			: type == typeof(short) ? Variant.Type.Int
			: type == typeof(int) ? Variant.Type.Int
			: type == typeof(long) ? Variant.Type.Int
			: type == typeof(byte) ? Variant.Type.Int
			: type == typeof(ushort) ? Variant.Type.Int
			: type == typeof(uint) ? Variant.Type.Int
			: type == typeof(ulong) ? Variant.Type.Int
			: type == typeof(float) ? Variant.Type.Float
			: type == typeof(double) ? Variant.Type.Float
			: type == typeof(Vector2) ? Variant.Type.Vector2
			: type == typeof(Vector2I) ? Variant.Type.Vector2I
			: type == typeof(Rect2) ? Variant.Type.Rect2
			: type == typeof(Rect2I) ? Variant.Type.Rect2I
			: type == typeof(Transform2D) ? Variant.Type.Transform2D
			: type == typeof(Vector3) ? Variant.Type.Vector3
			: type == typeof(Vector3I) ? Variant.Type.Vector3I
			: type == typeof(Basis) ? Variant.Type.Basis
			: type == typeof(Quaternion) ? Variant.Type.Quaternion
			: type == typeof(Transform3D) ? Variant.Type.Transform3D
			: type == typeof(Projection) ? Variant.Type.Projection
			: type == typeof(Vector4) ? Variant.Type.Vector4
			: type == typeof(Vector4I) ? Variant.Type.Vector4I
			: type == typeof(Aabb) ? Variant.Type.Aabb
			: type == typeof(Color) ? Variant.Type.Color
			: type == typeof(Plane) ? Variant.Type.Plane
			: type == typeof(Callable) ? Variant.Type.Callable
			: type == typeof(Signal) ? Variant.Type.Signal
			: type == typeof(string) ? Variant.Type.String
			: type == typeof(byte[]) ? Variant.Type.PackedByteArray
			: type == typeof(int[]) ? Variant.Type.PackedInt32Array
			: type == typeof(long[]) ? Variant.Type.PackedInt64Array
			: type == typeof(float[]) ? Variant.Type.PackedFloat32Array
			: type == typeof(double[]) ? Variant.Type.PackedFloat64Array
			: type == typeof(string[]) ? Variant.Type.PackedStringArray
			: type == typeof(Vector2[]) ? Variant.Type.PackedVector2Array
			: type == typeof(Vector3[]) ? Variant.Type.PackedVector3Array
			: type == typeof(Vector4[]) ? Variant.Type.PackedVector4Array
			: type == typeof(Color[]) ? Variant.Type.PackedColorArray
			: type == typeof(StringName[]) ? Variant.Type.PackedStringArray
			: type == typeof(NodePath[]) ? Variant.Type.PackedStringArray
			: type == typeof(Rid[]) ? Variant.Type.Array
			: type == typeof(StringName) ? Variant.Type.StringName
			: type == typeof(NodePath) ? Variant.Type.NodePath
			: type == typeof(Rid) ? Variant.Type.Rid
			: type == typeof(Godot.Collections.Dictionary) ? Variant.Type.Dictionary
			: type == typeof(Godot.Collections.Array) ? Variant.Type.Array
			: type == typeof(Variant) ? Variant.Type.Nil
			: typeof(GodotObject).IsAssignableFrom(type) ? Variant.Type.Object
			: type.IsValueType && typeof(Enum).IsAssignableFrom(type) ? Variant.Type.Int
			: Variant.Type.Nil;
	}

	extension <[MustBeVariant] T>(T value)
	{
		public Variant ToVariant() => Variant.From(value);
	}
}
