using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ExtensionMethods
{
    public static byte[] ToBytes(this Vector3 vector3)
    {
        List<byte> output = new List<byte>();
        output.AddRange(vector3.x.ToBytes());
        output.AddRange(vector3.y.ToBytes());
        output.AddRange(vector3.z.ToBytes());
        return output.ToArray();
    }

    public static Vector3 ToVector3(this byte[] bytes, int startPos)
    {
        if (startPos + 12 > bytes.Length - 1)
            throw new IndexOutOfRangeException();
        return new Vector3
        {
            x = bytes.ToFloat(startPos),
            y = bytes.ToFloat(startPos + 4),
            z = bytes.ToFloat(startPos + 8)
        };
    }

    /// <summary>
    /// A simple wrapper for easier interaction with bit converters.
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static byte[] ToBytes(this float value) => BitConverter.GetBytes(value);

    /// <summary>
    /// A simple wrapper for easier interaction with bit converters
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static byte[] ToBytes(this int value) => BitConverter.GetBytes(value);

    /// <summary>
    /// A simple wrapper for easier interaction with bit converters.
    /// </summary>
    /// <param name="bytes"></param>
    /// <param name="startPos"></param>
    /// <returns></returns>
    public static float ToFloat(this byte[] bytes, int startPos) => BitConverter.ToSingle(bytes, startPos);

    /// <summary>
    /// A simple wrapper to easily convert an integer to UShort.
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static ushort ToUShort(this int value) => Convert.ToUInt16(value);

    public static ushort ToUShort(this byte[] bytes, int startPos) => BitConverter.ToUInt16(bytes, startPos);

    public static long ToLong(this byte[] bytes, int startPos) => BitConverter.ToInt64(bytes, startPos);

    public static byte[] ToBytes(this ushort value) => BitConverter.GetBytes(value);

    public static void AddToArray(ref byte[] bytes, byte[] toAdd, int startPos)
    {
        if (startPos + toAdd.Length < bytes.Length)
            for (int i = 0; i < toAdd.Length; i++)
                bytes[i + startPos] = toAdd[i];
        else
            throw new IndexOutOfRangeException();
    }
}
