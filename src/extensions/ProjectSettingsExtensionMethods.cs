using Godot;

namespace Raele.GodotUtils.Extensions;

public static class ProjectSettingsExtensionMethods
{
	extension(ProjectSettings)
	{
		public static T GetSettingOrDefault<[MustBeVariant] T>(string setting)
			=> ProjectSettings.HasSetting(setting)
				? ProjectSettings.GetSetting(setting).As<T>()
				: default!;
		public static bool TryGetSetting<[MustBeVariant] T>(string setting, out T value)
		{
			bool hasSetting = ProjectSettings.HasSetting(setting);
			value = hasSetting ? ProjectSettings.GetSetting(setting).As<T>() : default!;
			return hasSetting;
		}
		public static void UpsertSetting(string setting, Variant defaultValue)
		{
			if (!ProjectSettings.HasSetting(setting))
			{
				ProjectSettings.SetSetting(setting, defaultValue);
			}
		}
	}
}