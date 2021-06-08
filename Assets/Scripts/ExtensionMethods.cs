using UnityEngine;

public static class ExtensionMethods
{
    public static bool IsBetween(this float value, float min, float max, bool inclusive = true) => inclusive ? value >= min && value <= max : value > min && value < max;

    public static float GetGrade(Vector3 pos1, Vector3 pos2)
    {
        float distance = Vector2.Distance(new Vector2(pos1.x, pos1.z), new Vector2(pos2.x, pos2.z));
        if (distance == 0)
            return 0;
        return (pos2.y - pos1.y) / distance * 100;
    }
}
