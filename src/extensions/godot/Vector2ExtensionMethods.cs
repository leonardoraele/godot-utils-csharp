using Godot;
using Raele.GodotUtils.Structs;

namespace Raele.GodotUtils.Extensions;

public static class Vector2ExtensionMethods
{
	extension(Vector2 self)
	{
		public bool IsParallelTo(Vector2 other)
			=> self.Dot(other)
				is > 1 - Mathf.Epsilon
				or < -1 + Mathf.Epsilon;
		public Vector2 MoveToward(Vector2 other, Radians delta)
			=> self + (other - self).LimitLength(delta);
		public Vector2 RotateToward(Vector2 other, Radians delta)
			=> other.IsZeroApprox()
				? self
				: self.Rotated(self.AngleTo(other).Clamped(-delta.AsFloat(), delta.AsFloat()));
		public Vector2 RotateAndScaleToward(Vector2 other, Radians angleDelta, float lengthDelta)
			=> self.RotateToward(other, angleDelta).Normalized()
				* self.Length().MoveToward(other.Length(), lengthDelta);

		public Vector2I AsVector2I()
			=> new Vector2I((int) self.X, (int) self.Y);
		public void Deconstruct(out float x, out float y)
		{
			x = self.X;
			y = self.Y;
		}
	}

	extension(Vector2I self)
	{
		public Vector2 AsVector2()
			=> new Vector2(self.X, self.Y);
		public void Deconstruct(out int x, out int y)
		{
			x = self.X;
			y = self.Y;
		}
	}
}
