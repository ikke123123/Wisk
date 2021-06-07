using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Level Rewards", menuName = "Level/Level Rewards")]
public class LevelRewards : ScriptableObject
{
    public LevelReward[] levelRewards = null;
}
