using System;
using System.Numerics;

namespace Raele.GodotUtils.Extensions;

public static class PrimitiveExtensionMethods
{
	extension<T>(T self) where T : struct, IComparable<T>
	{
		public T Clamped(T min, T max)
			=> self.CompareTo(min) < 0 ? min
				: self.CompareTo(max) > 0 ? max
				: self;
		public bool IsBetween(T min, T max, bool minInclusive = true, bool maxInclusive = false)
			=> (minInclusive ? self.CompareTo(min) >= 0 : self.CompareTo(min) > 0)
				&& (maxInclusive ? self.CompareTo(max) <= 0 : self.CompareTo(max) < 0);
	}

	extension<T>(T self) where T : struct, INumber<T>
	{
		public T Abs()
			=> T.Abs(self);
		public T MoveToward(T target, T delta)
			=> self < target ? T.Min(self + delta, target)
				: self > target ? T.Max(self - delta, target)
				: target;
	}
}
