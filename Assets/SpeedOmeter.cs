using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SpeedOmeter : MonoBehaviour
{
    public static SpeedOmeter sO = null;

    [SerializeField] private TextMeshProUGUI textMesh = null;

    private void Awake()
    {
        if (sO != null && sO != this)
            Destroy(sO);
        sO = this;
    }

    public static void SetSpeed(float speed)
    {
        sO.textMesh.text = speed.ToString("0.0") + " kph";
    }
}
