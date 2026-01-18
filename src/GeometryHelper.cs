using Godot;

namespace Raele.GodotUtils;

public static class GeometryHelper
{
	public static Vector2[] CreateRectanglePolygon(Rect2 rect)
		=>
		[
			new Vector2(rect.Position.X, rect.Position.Y),
			new Vector2(rect.Position.X + rect.Size.X, rect.Position.Y),
			new Vector2(rect.Position.X + rect.Size.X, rect.Position.Y + rect.Size.Y),
			new Vector2(rect.Position.X, rect.Position.Y + rect.Size.Y),
		];

	public static Vector2[] CreateCirclePolygon(Vector2 center, float radius, int segments = 32)
	{
		Vector2[] points = new Vector2[segments];
		float angleStep = Mathf.Tau / segments;
		for (int i = 0; i < segments; i++)
		{
			float angle = i * angleStep;
			points[i] = center + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
		}
		return points;
	}

	public static Vector2[] CreateCapsulePolygon(Vector2 center, float radius, float height, int segments = 16)
	{
		Vector2[] points = new Vector2[segments * 2 + 2];
		float angleStep = Mathf.Pi / segments;
		// Top semicircle
		for (int i = 0; i <= segments; i++)
		{
			float angle = Mathf.Pi + i * angleStep;
			points[i] = center + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius + new Vector2(0, -height / 2 + radius);
		}
		// Bottom semicircle
		for (int i = 0; i <= segments; i++)
		{
			float angle = i * angleStep;
			points[segments + 1 + i] = center + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius + new Vector2(0, height / 2 - radius);
		}
		return points;
	}
}
