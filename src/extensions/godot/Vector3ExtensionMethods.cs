using System;
using Godot;
using Raele.GodotUtils.Structs;

namespace Raele.GodotUtils.Extensions;

public static class Vector3ExtensionMethods
{
	extension(Vector3 self)
	{
		public bool IsParallelTo(Vector3 other)
			=> self.Dot(other)
				is > 1 - Mathf.Epsilon
				or < -1 + Mathf.Epsilon;
		public Vector3 MoveToward(Vector3 other, Radians delta)
			=> self + (other - self).LimitLength(delta);
		public Vector3 RotateToward(Vector3 other, Radians delta)
			=> self.RotateToward(other, delta, Vector3.Up);
		/// <summary>
		/// Rotates a vector toward another vector by <paramref name="delta"/> radians.
		/// The length of the rotated vector is not affected.
		/// </summary>
		/// <param name="defaultAxis">Rotates around this axis if the vectors are opposite to each other.</param>
		public Vector3 RotateToward(Vector3 other, Radians delta, Vector3 defaultAxis)
		{
			if (other.IsZeroApprox())
				return self;
			if (delta < 0)
			{
				other *= -1;
				delta *= -1;
			}
			float angle = self.AngleTo(other);
			if (angle < Mathf.Epsilon)
				return other;
			Vector3 cross = self.Cross(other);
			Vector3 axis = cross.IsZeroApprox()
				? defaultAxis
				: cross.Normalized();
			return self.Rotated(axis, Math.Min(delta, angle));
		}

		public void Deconstruct(out float x, out float y, out float z)
		{
			x = self.X;
			y = self.Y;
			z = self.Z;
		}
	}

	extension(Vector3I self)
	{
		public void Deconstruct(out int x, out int y, out int z)
		{
			x = self.X;
			y = self.Y;
			z = self.Z;
		}
	}
}
