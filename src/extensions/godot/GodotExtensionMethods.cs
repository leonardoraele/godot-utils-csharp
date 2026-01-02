using System.Collections.Generic;
using System.Linq;
using Godot;

namespace Raele.GodotUtils.Extensions;

public static class GodotExtensionMethods
{
	extension(Engine)
	{
		public static SceneTree GetSceneTree() => (SceneTree) Engine.GetMainLoop();
	}

	extension<[MustBeVariant] T>(IEnumerable<T> sequence)
	{
		public Godot.Collections.Array ToGodotArray() => [..sequence.Select(item => Variant.From(item))];
		public Godot.Collections.Array<T> ToGodotArrayT() => [..sequence];
	}

	extension<[MustBeVariant] TKey, [MustBeVariant] TValue>(Dictionary<TKey, TValue> dictionary) where TKey : notnull
	{
		public Godot.Collections.Dictionary ToGodotDictionary()
		{
			Godot.Collections.Dictionary godotDict = new();
			foreach (KeyValuePair<TKey, TValue> pair in dictionary)
			{
				godotDict[Variant.From(pair.Key)] = Variant.From(pair.Value);
			}
			return godotDict;
		}
		public Godot.Collections.Dictionary<TKey, TValue> ToGodotDictionaryKV()
		{
			Godot.Collections.Dictionary<TKey, TValue> godotDict = new();
			foreach (KeyValuePair<TKey, TValue> pair in dictionary)
			{
				godotDict[pair.Key] = pair.Value;
			}
			return godotDict;
		}
	}
}
