using UnityEngine;

public class MapConnection
{
    public MapNode from;
    public MapNode to;

    public bool Intersects(MapConnection other)
    {
        // 자기 자신과는 교차하지 않음
        if (from == other.from || from == other.to || to == other.from || to == other.to)
            return false;

        Vector2 p1 = new Vector2(from.x, from.y);
        Vector2 p2 = new Vector2(to.x, to.y);
        Vector2 p3 = new Vector2(other.from.x, other.from.y);
        Vector2 p4 = new Vector2(other.to.x, other.to.y);

        return DoLinesIntersect(p1, p2, p3, p4);
    }

    private bool DoLinesIntersect(Vector2 a1, Vector2 a2, Vector2 b1, Vector2 b2)
    {
        float d = (a2.x - a1.x) * (b2.y - b1.y) - (a2.y - a1.y) * (b2.x - b1.x);
        if (Mathf.Approximately(d, 0)) return false;

        float u = ((b1.x - a1.x) * (b2.y - b1.y) - (b1.y - a1.y) * (b2.x - b1.x)) / d;
        float v = ((b1.x - a1.x) * (a2.y - a1.y) - (b1.y - a1.y) * (a2.x - a1.x)) / d;

        return (u > 0 && u < 1 && v > 0 && v < 1);
    }
}
