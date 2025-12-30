using Godot;

namespace Raele.GodotUtils.Extensions;

public static class GodotExtensionMethods
{
	extension(Engine)
	{
		public static SceneTree GetSceneTree() => (SceneTree) Engine.GetMainLoop();
	}
}
