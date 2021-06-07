using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Quest", menuName = "Quests/Quest")]
public class Quest : ScriptableObject
{
    public string questID = "";
    public string questName = "";
    public string questDescription = "";

    public Reward[] rewards = null;

    public QuestElement[] questElements = null;

    public LocationTrigger locationTrigger = null;

    [Header("Debug")]
    public Status status = Status.Locked;
    [SerializeField] private QuestUIManager questUIManager = null;

    public Status CheckStatus(bool save)
    {
        if (save) 
            status = (Status)PlayerPrefs.GetInt(questID, 3);
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

    public Status SetStatus(Status status, bool save)
    {
        this.status = status;

        switch ((int)this.status)
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
            PlayerPrefs.SetInt(questID, (int)status);

        return this.status;
    }

    #region Loaded

    internal void OnActiveLoaded()
    {
        questUIManager = QuestUIManager.GetUIManager();

        foreach (QuestElement questElement in questElements)
        {
            questElement.questUIManager = questUIManager;
        }

        questUIManager.SetQuest(questName);
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
        questUIManager.ReleaseUIManager();
        questUIManager = null;
    }

    private void OnCompleted()
    {
        questUIManager.ReleaseUIManager();
        questUIManager = null;

        foreach (Reward reward in rewards)
            reward.GiveReward();
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
        questUIManager = QuestUIManager.GetUIManager();

        foreach (QuestElement questElement in questElements)
        {
            questElement.questUIManager = questUIManager;
        }

        questUIManager.SetQuest(questName);

        //ADD CHECK FOR FIRST TIME DEPENDENCIES
        QuestManager.SetQuestElementStatus(questElements[0].questElementID, Status.Active);
    }

    #endregion
}
