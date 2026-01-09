namespace Raele.GodotUtils.Input;

public enum InputActionEnum : byte
{
	None = 0,
	InputIsJustPressed = 32,
	InputIsPressed = 64,
	InputIsJustReleased = 96,
	InputIsReleased = 128,
}

public static class InputActionEnumExtensions
{
	extension (InputActionEnum actionType)
	{
		public bool Test(string actionName)
			=> !string.IsNullOrWhiteSpace(actionName)
				&& actionType switch
				{
					InputActionEnum.InputIsJustPressed => Godot.Input.IsActionJustPressed(actionName),
					InputActionEnum.InputIsPressed => Godot.Input.IsActionPressed(actionName),
					InputActionEnum.InputIsJustReleased => Godot.Input.IsActionJustReleased(actionName),
					InputActionEnum.InputIsReleased => !Godot.Input.IsActionPressed(actionName),
					_ => false,
				};
	}
}
