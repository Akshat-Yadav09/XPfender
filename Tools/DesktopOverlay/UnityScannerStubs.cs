// Used only by the external scanner probe; never imported into Unity.
namespace UnityEngine
{
    public class GameObject { }
    public class TooltipAttribute : System.Attribute { public TooltipAttribute(string value) { } }
    public struct Vector2
    {
        public float x, y;
        public Vector2(float x, float y) { this.x = x; this.y = y; }
        public override string ToString() { return "(" + x + ", " + y + ")"; }
    }
    public struct Rect
    {
        public float x, y, width, height;
        public Rect(float x, float y, float width, float height) { this.x = x; this.y = y; this.width = width; this.height = height; }
    }
    public struct Vector3 { }
    public class Camera { }
    public static class Debug
    {
        public static void Log(object value) { System.Console.WriteLine(value); }
        public static void LogError(object value) { System.Console.WriteLine("ERROR: " + value); }
        public static void LogWarning(object value) { System.Console.WriteLine("WARNING: " + value); }
    }
}
