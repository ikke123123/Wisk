using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Level Reward", menuName = "Level/Level Reward")]
public class LevelReward : ScriptableObject
{
    public Reward[] rewards = null;
}
