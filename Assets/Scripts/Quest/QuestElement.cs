using System;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "New Quest Element", menuName = "Quests/Quest Element")]
public class QuestElement : ScriptableObject
{
    public string questID = "";
    public string questElementID = "";
    public string questElementName = "";
    public string questElementDescription = "";

    public Dependency dependency = null;

    public bool useRewards = false;
    public Reward[] rewards = null;

    //Dialogue
    public bool useDialogue = false;
    public BasicDialogue[] dialogue = null;

    //Location Trigger
    public bool useLocationTrigger = false;
    public LocationTrigger locationTriggerPrefab;
    public LocationTriggerData locationTrigger;
    

    [Header("Debug")]
    public Status status = Status.Locked;
    public QuestUIManager questUIManager = null;
    [SerializeField] private int questUIManagerSpot = 0;
    public GameObject locationTriggerGameObject = null;

    public Status CheckStatus(bool save = true)
    {
        if (save) status = (Status)PlayerPrefs.GetInt(questID, 3);
        else status = Status.Locked;

        switch ((int)status)
        {
            case 0: //Active
                OnActiveLoaded();
                break;
            case 1: //Unlocked
                OnUnlockedLoaded();
                break;
            case 2: //Locked
                OnLockedLoaded();
                break;
            case 3: //Completed
                OnCompletedLoaded();
                break;
            case 4: //Failed
                OnFailedLoaded();
                break;
        }

        return status;
    }

    public Status SetStatus(Status status, bool save = true)
    {
        this.status = status;

        switch ((int)status)
        {
            case 0: //Active
                OnActive();
                break;
            case 1: //Unlocked
                OnUnlocked();
                break;
            case 2: //Locked
                OnLocked();
                break;
            case 3: //Completed
                OnCompleted();
                break;
            case 4: //Failed
                OnFailed();
                break;
        }

        if (save)
            PlayerPrefs.SetInt(questElementID, (int)status);

        return status;
    }

    #region Loaded
    internal void OnActiveLoaded()
    {
        questUIManagerSpot = questUIManager.GetElementSpot();
        questUIManager.SetQuestElement(questElementName, questUIManagerSpot);

        if (useLocationTrigger)
            locationTriggerPrefab.Spawn(locationTrigger, true);
    }

    internal void OnUnlockedLoaded()
    {
        Debug.LogError("Not implemented");
    }

    internal void OnLockedLoaded()
    {
        Debug.LogError("Not implemented");
    }

    internal void OnCompletedLoaded()
    {
        Debug.LogError("Not implemented");
    }

    internal void OnFailedLoaded()
    {
        Debug.LogError("Not implemented");
    }

    #endregion

    #region Events

    private void OnFailed()
    {
        questUIManager.ReleaseElementSpot(questUIManagerSpot);
        questUIManagerSpot = 0;

        if (locationTriggerGameObject != null)
            Destroy(locationTriggerGameObject);
    }

    private void OnCompleted()
    {
        questUIManager.ReleaseElementSpot(questUIManagerSpot);
        questUIManagerSpot = 0;

        foreach (Reward reward in rewards)
            reward.GiveReward();

        if (locationTriggerGameObject != null)
            Destroy(locationTriggerGameObject);
    }

    private void OnLocked()
    {
        Debug.LogError("Not implemented");
    }

    private void OnUnlocked()
    {
        Debug.LogError("Not implemented");
    }

    private void OnActive()
    {
        questUIManagerSpot = questUIManager.GetElementSpot();
        questUIManager.SetQuestElement(questElementName, questUIManagerSpot);

        if (useLocationTrigger == true)
            locationTriggerPrefab.Spawn(locationTrigger, true);

        if (useDialogue)
            foreach (BasicDialogue basicDialogue in dialogue)
                Dialogue.AddText(basicDialogue.title, basicDialogue.text);
    }

    #endregion
}