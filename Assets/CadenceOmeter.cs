using System.Collections;
using System.Collections.Generic;
using ThomasLib.Num;
using UnityEngine;
using UnityEngine.UI;

public class CadenceOmeter : MonoBehaviour
{
    [SerializeField] private Image image;

    public void SetValue(float input)
    {
        image.fillAmount = NumTool.Remap(Mathf.Clamp(input, 0, 150), 0, 150, 0, 1);
    }
}
