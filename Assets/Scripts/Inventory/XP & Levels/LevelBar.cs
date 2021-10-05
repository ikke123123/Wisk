using System.Collections;
using System.Collections.Generic;
using ThomasLib.Num;
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

    private void Awake()
    {
        if (lB != null & lB != this)
        {
            Destroy(lB);
        }
        lB = this;
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
        int currentLevel = XPManager.Level;
        int currentXP = XPManager.XP;
        int currentLevelTarget = XPManager.CurrentLevelTarget;

        levelNumber.text = currentLevel.ToString();
        targetLevelNumber.text = (currentLevel + 1).ToString();
        currentXPValue.text = currentXP.ToString() + " XP";
        targetXPValue.text = currentLevelTarget.ToString() + " XP";
        image.fillAmount = XPManager.LevelT;
    }
}
