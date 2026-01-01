using System;

namespace Raele.GodotUtils.Extensions;

public static class PrimitiveExtensionMethods
{
	extension<T>(T self) where T : struct, IComparable<T>
	{
		public T Clamp(T min, T max)
			=> self.CompareTo(min) < 0 ? min
				: self.CompareTo(max) > 0 ? max
				: self;
		public bool IsBetween(T min, T max, bool inclusive = true)
			=> inclusive
				? self.CompareTo(min) >= 0 && self.CompareTo(max) <= 0
				: self.CompareTo(min) > 0 && self.CompareTo(max) < 0;
	}

	extension(string str)
	{
		public string TrimEnd(string suffix)
		{
			if (str.EndsWith(suffix, StringComparison.Ordinal))
			{
				return str.Substring(0, str.Length - suffix.Length).TrimEnd(suffix);
			}
			return str;
		}
		public string TrimStart(string prefix)
		{
			if (str.StartsWith(prefix, StringComparison.Ordinal))
			{
				return str.Substring(prefix.Length).TrimStart(prefix);
			}
			return str;
		}

		public string BBCBold() => $"[b]{str}[/b]";
		public string BBCItalic() => $"[i]{str}[/i]";
		public string BBCStrikethrough() => $"[s]{str}[/s]";
		public string BBCUnderscore() => $"[u]{str}[/u]";
		public string BBCCode() => $"[code]{str}[/code]";
		public string BBCUrl(string url) => $"[url={url}]{str}[/url]";
		public string BBCCenter() => $"[center]{str}[/center]";
		public string BBCRight() => $"[right]{str}[/right]";
		public string BBCColor(string color) => $"[color={color}]{str}[/color]";
		public string BBCBgColor(string color) => $"[bgcolor={color}]{str}[/bgcolor]";
		public string BBCFgColor(string color) => $"[fgcolor={color}]{str}[/fgcolor]";
	}
}
