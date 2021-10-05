using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class MonoBehaviourOmeter : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI textMesh = null;
    [SerializeField] private string suffix = "";
    [SerializeField] private string toStringParameters = "";

    private void Start()
    {
        SetValue(0);
    }

    public void SetValue(float input)
    {
        textMesh.text = (toStringParameters != "" ? input.ToString(toStringParameters) : input.ToString()) + " " + suffix;
    }
}

