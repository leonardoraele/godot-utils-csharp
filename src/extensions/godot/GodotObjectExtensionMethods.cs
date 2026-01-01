using System;
using Godot;

namespace Raele.GodotUtils.Extensions;

public static class GodotObjectExtensionMethods
{
	private const double DEFAULT_DEBOUNCE_DELAY_SECONDS = 0.200d;
	private const double DEFAULT_THROTTLE_DELAY_SECONDS = 0.200d;

	extension(GodotObject self)
	{
		public bool IsInstanceValid() => GodotObject.IsInstanceValid(self);

		public void CallDebouncedRealTime(TimeSpan delay, StringName methodName, params Variant[] args)
			=> self._CallDebounced(methodName, delay.TotalSeconds, ignoreTimeScale: true, args);
		public void CallDebounced(TimeSpan delay, StringName methodName, params Variant[] args)
			=> self._CallDebounced(methodName, delay.TotalSeconds, ignoreTimeScale: false, args);
		public void CallDebouncedRealTime(double delaySeconds, StringName methodName, params Variant[] args)
			=> self._CallDebounced(methodName, delaySeconds, ignoreTimeScale: true, args);
		public void CallDebounced(double delaySeconds, StringName methodName, params Variant[] args)
			=> self._CallDebounced(methodName, delaySeconds, ignoreTimeScale: false, args);
		public void CallDebouncedRealTime(StringName methodName, params Variant[] args)
			=> self._CallDebounced(methodName, DEFAULT_DEBOUNCE_DELAY_SECONDS, ignoreTimeScale: true, args);
		public void CallDebounced(StringName methodName, params Variant[] args)
			=> self._CallDebounced(methodName, DEFAULT_DEBOUNCE_DELAY_SECONDS, ignoreTimeScale: false, args);
		/// <summary>
		/// Calls a method on the GodotObject after a delay, resetting the delay if called again before the timer
		/// elapses.
		/// </summary>
		private void _CallDebounced(
			StringName methodName,
			double delaySeconds = DEFAULT_DEBOUNCE_DELAY_SECONDS,
			bool ignoreTimeScale = false,
			params Variant[] args
		)
		{
			string timerName = $"_{nameof(_CallDebounced)}_{self.GetInstanceId()}_{methodName}";
			Timer? timer = Engine.GetSceneTree().Root.GetNodeOrNull<Timer>(timerName);
			if (timer == null)
			{
				timer = new Timer() { Name = timerName, OneShot = true, IgnoreTimeScale = ignoreTimeScale };
				timer.Timeout += () =>
				{
					timer.QueueFree();
					if (self.IsInstanceValid())
					{
						self.Call(methodName, args);
					}
				};
				Engine.GetSceneTree().Root.AddChild(timer);
			}
			timer.Start(delaySeconds);
		}

		public void CallThrottledRealTime(TimeSpan delay, StringName methodName, params Variant[] args)
			=> self._CallThrottled(methodName, delay.TotalSeconds, ignoreTimeScale: true, args);
		public void CallThrottled(TimeSpan delay, StringName methodName, params Variant[] args)
			=> self._CallThrottled(methodName, delay.TotalSeconds, ignoreTimeScale: false, args);
		public void CallThrottledRealTime(double delaySeconds, StringName methodName, params Variant[] args)
			=> self._CallThrottled(methodName, delaySeconds, ignoreTimeScale: true, args);
		public void CallThrottled(double delaySeconds, StringName methodName, params Variant[] args)
			=> self._CallThrottled(methodName, delaySeconds, ignoreTimeScale: false, args);
		public void CallThrottledRealTime(StringName methodName, params Variant[] args)
			=> self._CallThrottled(methodName, DEFAULT_THROTTLE_DELAY_SECONDS, ignoreTimeScale: true, args);
		public void CallThrottled(StringName methodName, params Variant[] args)
			=> self._CallThrottled(methodName, DEFAULT_THROTTLE_DELAY_SECONDS, ignoreTimeScale: false, args);

		/// <summary>
		/// Calls a method on the GodotObject after a delay, ignoring subsequent calls until the timer elapses.
		/// </summary>
		private void _CallThrottled(
			StringName methodName,
			double delaySeconds = DEFAULT_THROTTLE_DELAY_SECONDS,
			bool ignoreTimeScale = false,
			params Variant[] args
		)
		{
			string timerName = $"_{nameof(_CallThrottled)}_{self.GetInstanceId()}_{methodName}";
			Timer? timer = Engine.GetSceneTree().Root.GetNodeOrNull<Timer>(timerName);
			if (timer == null)
			{
				timer = new Timer() { Name = timerName, OneShot = true, IgnoreTimeScale = ignoreTimeScale };
				timer.Timeout += timer.QueueFree;
				Engine.GetSceneTree().Root.AddChild(timer);
				timer.Start(delaySeconds);
				self.Call(methodName, args);
			}
		}

		[Obsolete("Use CallDebounced(GodotObject.MethodName.EmitSignal, ...) or CallDebouncedRealTime(...) instead.")]
		public void EmitSignalDebounced(StringName signalName, float delaySeconds = 0.200f, bool ignoreTimeScale = false, params Variant[] args)
			=> self._CallDebounced(signalName, delaySeconds, ignoreTimeScale, args);
		[Obsolete("Use CallDebounced(GodotObject.MethodName.EmitSignal, ...) or CallDebouncedRealTime(...) instead.")]
		public void EmitSignalThrottled(StringName signalName, float delaySeconds = 0.200f, bool ignoreTimeScale = false, params Variant[] args)
		{
			string timerName = $"_{nameof(EmitSignalThrottled)}_{self.GetInstanceId()}_{signalName}";
			Timer? timer = Engine.GetSceneTree().Root.GetNodeOrNull<Timer>(timerName);
			if (timer == null)
			{
				timer = new Timer() { Name = timerName, OneShot = true, IgnoreTimeScale = ignoreTimeScale };
				timer.Timeout += timer.QueueFree;
				Engine.GetSceneTree().Root.AddChild(timer);
				timer.Start(delaySeconds);
				self.EmitSignal(signalName, args);
			}
		}
	}
}
