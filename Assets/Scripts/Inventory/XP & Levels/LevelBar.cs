using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LevelBar : MonoBehaviour
{
    public static LevelBar lB = null;

    [SerializeField] private TextMeshProUGUI levelNumber = null;
    [SerializeField] private TextMeshProUGUI targetLevelNumber = null;
    [SerializeField] private TextMeshProUGUI currentXPValue = null;
    [SerializeField] private TextMeshProUGUI targetXPValue = null;
    [SerializeField] private Image image = null;

    private float maxWidth;

    private void Awake()
    {
        if (lB != null & lB != this)
        {
            Destroy(lB);
        }
        lB = this;
    }

    private void Start()
    {
        maxWidth = image.rectTransform.rect.width;
    }

    /// <summary>
    /// Protected version of UpdateText.
    /// </summary>
    public static void CallUpdate()
    {
        if (lB != null) lB.UpdateText();
    }

    public void UpdateText()
    {
        levelNumber.text = XPManager.Level.ToString();
        targetLevelNumber.text = (XPManager.Level + 1).ToString();
        currentXPValue.text = XPManager.XP.ToString();
        targetXPValue.text = XPManager.CurrentLevelTarget.ToString();
        image.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, Mathf.Lerp(0, maxWidth, XPManager.LevelT));
    }
}
