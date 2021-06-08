using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GradeOmeter : MonoBehaviour
{
    public static GradeOmeter gO = null;

    [SerializeField] private TextMeshProUGUI textMesh = null;

    private void Awake()
    {
        if (gO != null && gO != this)
            Destroy(gO);
        gO = this;
    }

    public static void SetGrade(float grade)
    {
        gO.textMesh.text = grade.ToString("0.0") + " %";
    }
}

