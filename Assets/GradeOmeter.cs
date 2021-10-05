using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GradeOmeter : MonoBehaviour
{
    [SerializeField] private RectTransform gradeVisualizer = null;
    [SerializeField] private MonoBehaviourOmeter gradeText = null;

    public void SetValue(float input)
    {
        gradeVisualizer.rotation = Quaternion.Euler(0, 0, Mathf.Rad2Deg * Mathf.Atan(input/100));
        gradeText.SetValue(input);
    }
}
