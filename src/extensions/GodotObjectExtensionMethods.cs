using Godot;

namespace Raele.GodotUtils.Extensions;

public static class GodotObjectExtensionMethods
{
	extension(GodotObject self)
	{
		public bool IsInstanceValid() => GodotObject.IsInstanceValid(self);
		public void EmitSignalDebounced(StringName signalName, float delaySeconds = 0.200f, bool ignoreTimeScale = false, params Variant[] args)
		{
			string timerName = $"_{nameof(EmitSignalDebounced)}_{self.GetInstanceId()}_{signalName}";
			Timer? timer = Engine.GetSceneTree().Root.GetNodeOrNull<Timer>(timerName);
			if (timer == null)
			{
				timer = new Timer() { Name = timerName, OneShot = true, IgnoreTimeScale = ignoreTimeScale };
				timer.Timeout += () =>
				{
					timer.QueueFree();
					if (self.IsInstanceValid())
					{
						self.EmitSignal(signalName, args);
					}
				};
				Engine.GetSceneTree().Root.AddChild(timer);
			}
			timer.Start(delaySeconds);
		}
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
