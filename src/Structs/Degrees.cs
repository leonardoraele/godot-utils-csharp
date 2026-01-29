using Godot;

namespace Raele.GodotUtils.Structs;

public struct Degrees(double VALUE)
{
	public double ToRadians()
		=> Mathf.DegToRad(VALUE);

	public double AsDouble() => VALUE;
	public float AsFloat() => (float) VALUE;
	public Variant AsVariant() => VALUE;

	public static implicit operator double(Degrees degrees) => degrees.AsDouble();
	public static implicit operator Degrees(double value) => new Degrees(value);
	public static implicit operator float(Degrees degrees) => degrees.AsFloat();
	public static implicit operator Degrees(float value) => new Degrees(value);
	public static implicit operator Variant(Degrees degrees) => degrees.AsDouble();
}

public static class DegreesExtensionMethods
{
	public static Degrees AsDegrees(this float value) => value;
	public static Degrees AsDegrees(this double value) => value;
	public static Degrees AsDegrees(this Variant value) => value.AsDouble();
}
