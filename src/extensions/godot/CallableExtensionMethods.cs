using System;
using Godot;

namespace Raele.GodotUtils.Extensions;

public static class CallableExtensionMethods
{
	extension(Delegate self)
	{
		public Callable ToCallable()
		{
			if (self.Target is not GodotObject target)
				throw new InvalidOperationException($"Failed to convert delegate {self.GetType().Name} to Godot.Callable. Cause: Mandatory requirement not met. Requirement: Delegate target must be a GodotObject.");
			return new Callable(target, self.Method.Name);
		}
	}
}
