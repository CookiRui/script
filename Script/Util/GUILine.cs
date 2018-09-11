/*
    author:jlx
    http://wiki.unity3d.com/index.php/DrawLine
*/

using UnityEngine;

public static class GUILine
{
    static Texture2D lineTex;

    public static void draw(Rect rect) { draw(rect, GUI.contentColor, 1.0f); }
    public static void draw(Rect rect, Color color) { draw(rect, color, 1.0f); }
    public static void draw(Rect rect, float width) { draw(rect, GUI.contentColor, width); }
    public static void draw(Rect rect, Color color, float width)
    {
        var point1 = new Vector2 { x = rect.xMin, y = rect.yMin };
        var point2 = new Vector2 { x = rect.xMin, y = rect.yMax };
        var point3 = new Vector2 { x = rect.xMax, y = rect.yMax };
        var point4 = new Vector2 { x = rect.xMax, y = rect.yMin };
        draw(point1, point2, color, width);
        draw(point2, point3, color, width);
        draw(point3, point4, color, width);
        draw(point4, point1, color, width);
    }
    public static void draw(Vector2 pointA, Vector2 pointB) { draw(pointA, pointB, GUI.contentColor, 1.0f); }
    public static void draw(Vector2 pointA, Vector2 pointB, Color color) { draw(pointA, pointB, color, 1.0f); }
    public static void draw(Vector2 pointA, Vector2 pointB, float width) { draw(pointA, pointB, GUI.contentColor, width); }
    public static void draw(Vector2 pointA, Vector2 pointB, Color color, float width)
    {
        if (!lineTex) { lineTex = new Texture2D(1, 1); }
        Color savedColor = GUI.color;

        Vector2 d = pointB - pointA;
        float a = Mathf.Rad2Deg * Mathf.Atan(d.y / d.x);
        if (d.x < 0)
            a += 180;

        int width2 = (int)Mathf.Ceil(width / 2);

        GUIUtility.RotateAroundPivot(a, pointA);
        GUI.color = color;
        GUI.DrawTexture(new Rect(pointA.x, pointA.y - width2, d.magnitude, width), lineTex);
        GUIUtility.RotateAroundPivot(-a, pointA);
        GUI.color = savedColor;
    }
}