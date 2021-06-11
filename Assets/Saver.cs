using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class Saver : MonoBehaviour
{
    [SerializeField, Tooltip("In time in seconds.")] private float pollFrequency = 1;

    /// <summary>
    /// Turn off poll data and save it safely.
    /// </summary>
    public bool active = false;

    private MemoryStream mS = null;

    private IEnumerator PollData()
    {
        using (mS = new MemoryStream())
        {
            mS.WriteAsync(DateTime.UtcNow);
            while (active)
            {

                yield return new WaitForSeconds(pollFrequency);
            }
        }
    }
}
