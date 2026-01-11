using System;
using Godot;

namespace Raele.GodotUtils.Extensions;

public static class LoggingExtensions
{
	// public static void DebugLog(this Node subject, params object[] items)
	// 	=> GD.PrintRich(
	// 		string.Join(" ", items.SkipLast(1)),
	// 		items.Length > 1
	// 			? "\n\t " + items.Last()
	// 			: "",
	// 		"\n\t ", GetSourceString(subject)
	// 	);
	public static void DebugLog(this Node subject, string message)
		=> GD.PrintRich(
			message,
			"\n\t ", GetSourceString(subject)
		);
	public static void DebugLog(this Node subject, string message, object detailsObj)
		=> GD.PrintRich(
			message,
			"\n\t ", detailsObj,
			"\n\t ", GetSourceString(subject)
		);
	public static void DebugLog(this Node subject, string message, Variant details)
		=> GD.PrintRich(
			message,
			"\n\t ", Json.Stringify(Variant.From(details)),
			"\n\t ", GetSourceString(subject)
		);

	private static string GetSourceString(Node subject)
		=> string.Join(
				" ",
				[
					DateTime.Now.ToString("HH:mm:ss.fff"),
					$"#{Engine.GetPhysicsFrames():N0}",
					"by",
					$"\"{subject.GetPath().ToString().TrimEnd(subject.Name)}{subject.Name.ToString().BBCColor(ColorName.Gray)}\""
				]
			)
			.BBCColor(ColorName.DarkGray);
}
