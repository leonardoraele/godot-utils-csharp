using Godot;
using Raele.GodotUtils.Structs;

namespace Raele.GodotUtils.Extensions;

public static class BasisExtensionMethods
{
	extension(Basis self)
	{
		public Vector3 Up => self.Y;
		public Vector3 Down => -self.Y;
		public Vector3 Right => self.X;
		public Vector3 Left => -self.X;
		public Vector3 Forward => -self.Z;
		public Vector3 Back => self.Z;
		public Basis RotateToward(Vector3 target, Radians deltaAngle, Vector3? upDirection = null)
		{
			upDirection ??= Vector3.Up;
			Vector3 newDirection = self.Forward.RotateToward(target, deltaAngle, upDirection.Value).Normalized();
			return Basis.LookingAt(newDirection).Scaled(self.Scale);
		}
	}
}
