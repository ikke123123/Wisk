using System;
using System.Collections;
using System.Collections.Generic;
using ThomasLib.Num;
using UnityEngine;

public class XPManager : MonoBehaviour
{
    public static XPManager xPM = null;

    //Absolute
    /// <summary>
    /// Current Level.
    /// </summary>
    public static int Level => xPM.level;
    /// <summary>
    /// Current absolute XP.
    /// </summary>
    public static int XP => xPM.xp;
    /// <summary>
    /// Absolute target for level XP.
    /// </summary>
    public static int CurrentLevelTarget => xPM.currentLevelTarget;
    /// <summary>
    /// Absolute floor of the current level XP.
    /// </summary>
    public static int CurrentLevelFloor => xPM.currentLevelFloor;


    //Relative
    /// <summary>
    /// XP within this level.
    /// </summary>
    public static int LevelXP => xPM.xp - xPM.currentLevelFloor;
    /// <summary>
    /// T of current xp between currentLevelFloor and currentLevelTarget. Ranges from 0 to 1.
    /// </summary>
    public static float LevelT => Mathf.Clamp(NumTool.Remap(LevelXP, LevelFloor, LevelTarget, 0, 1), 0, 1);
    /// <summary>
    /// Relative target for level XP.
    /// </summary>
    public static int LevelTarget => xPM.currentLevelTarget - xPM.currentLevelFloor;
    /// <summary>
    /// Relative floor of the current level XP.
    /// </summary>
    public static int LevelFloor => 0;

    [SerializeField] private string xpSave = "TotalXP";
    [SerializeField] private string levelSave = "Level";
    [SerializeField] private int floor = 150; //Required xp for the first level.
    [SerializeField, Range(1, 2f)] private float modifierPerLevel = 1.1f;
    [SerializeField] private LevelRewards levelRewards;

    [Header("Debug")]
    [SerializeField] private bool saveEnabled = false;
    [SerializeField] private int xp = 150;
    [SerializeField] private int level = 1;
    [SerializeField] private int currentLevelTarget = 0; //If beyond this you reach next level
    [SerializeField] private int currentLevelFloor = 0; //The xp from this level

    [Header("Test")]
    [SerializeField] private bool commit = false;
    [SerializeField] private int addXP = 0;

    private void Awake()
    {
        if (xPM != null & xPM != this)
        {
            Destroy(xPM);
        }
        xPM = this;
    }

    private void Start()
    {
        if (saveEnabled)
        {
            xp = PlayerPrefs.GetInt(xpSave, 0);
            level = PlayerPrefs.GetInt(levelSave, 1);
        }
        else
        {
            xp = 0;
            level = 1;
        }
        currentLevelFloor = Mathf.RoundToInt((float)(floor * (level == 1 ? 0 : Math.Pow(modifierPerLevel, level - 2))));
        currentLevelTarget = Mathf.RoundToInt((float)(floor * (level == 1 ? 1 : Math.Pow(modifierPerLevel, level - 1))));
        LevelBar.CallUpdate();
    }

    private void Update()
    {
        if (commit)
        {
            commit = false;
            Add(addXP);
        }
    }

    public void Add(int xp)
    {
        this.xp += xp;
        if (saveEnabled) PlayerPrefs.SetInt(xpSave, this.xp);

        while (this.xp >= currentLevelTarget)
        {
            LevelUp();
        }
        LevelBar.CallUpdate();
    }

    private void LevelUp()
    {
        level++;

        currentLevelFloor = currentLevelTarget;
        currentLevelTarget += Mathf.RoundToInt((float)(floor * Math.Pow(modifierPerLevel, level - 1)));

        if (saveEnabled) PlayerPrefs.SetInt(levelSave, level);
        
        if (levelRewards != null && level - 1 < levelRewards.levelRewards.Length)
        {
            foreach (Reward reward in levelRewards.levelRewards[level - 1].rewards) reward.GiveReward();
        }

        MessageManager.SendMessage(MessageManager.TypeOf.Inventory, "Reached Level " + level);
    }
}