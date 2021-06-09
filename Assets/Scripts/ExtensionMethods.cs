using System.Collections;
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

    public static bool GetBit(this byte input, int bitIndex) => (input & (1 << bitIndex - 1)) != 0;


    public static uint GetUInt24(this byte[] bytes, int startIndex)
    {
        uint output = 0;
        for (int i = 0; i < 3; i++)
        {
            uint tempNumber = bytes[startIndex + i];
            output += tempNumber << 8 * i;
        }
        return output;
    }
}
