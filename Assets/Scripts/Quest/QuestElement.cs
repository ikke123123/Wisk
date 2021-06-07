using System;
using UnityEngine;

[CreateAssetMenu(fileName = "New Quest Element", menuName = "Quests/Quest Element")]
public class QuestElement : ScriptableObject
{
    public string questID = "";
    public string questElementID = "";
    public string questElementName = "";
    public string questElementDescription = "";
    public int currentSubquestElement = 0;

    public Dependency dependency = null;

    public SubQuestElement[] subQuestElements = null;
    public Reward[] rewards = null;

    public LocationTrigger locationTrigger = null;

    [Header("Debug")]
    public Status status = Status.Locked;
    public QuestUIManager questUIManager = null;
    [SerializeField] private int questUIManagerSpot = 0;

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
        Debug.LogError("Not implemented");
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
    }

    private void OnCompleted()
    {
        questUIManager.ReleaseElementSpot(questUIManagerSpot);
        questUIManagerSpot = 0;

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
        questUIManagerSpot = questUIManager.GetElementSpot();
        questUIManager.SetQuestElement(questElementName, questUIManagerSpot);
    }

    #endregion
}

[Serializable]
public class Dependency
{
    public enum Dependencies { Linear, ParallelConstraints, ParallelCompletion };

    public Dependencies dependencies;

    //For linear & parallel constraints:
    [Tooltip("This one will be set to active/unlocked upon finishing the thing.")] public QuestElement[] nextQuestElementIDs = null;
    [Tooltip("If not completed already, these elements will be set to failed, as they were not completed.")] public QuestElement[] questElementIDsFailed = null;

    //For parallel constraints:
    [Tooltip("Before moving on to the next quest this checks for these to be completed as well, otherwise it can't move on.")] public QuestElement[] questElementIDs = null;

    [HideInInspector] public QuestElement myElement = null;

    /// <summary>
    /// Returns true if the current quest step was not completed.
    /// </summary>
    /// <returns></returns>
    public bool OnCompleted()
    {
        switch ((int)dependencies)
        {
            case 0: //Linear
                return LinearCheck();
            case 1: //Parallel Constraints
                return ParallelConstraintsCheck();
            case 2: //Parallel Completion
                return true;
        }
        return false;
    }

    private bool LinearCheck()
    {
        foreach (QuestElement questElement in questElementIDsFailed)
        {
            if (questElement.status != Status.Completed)
            {
                QuestManager.SetQuestElementStatus(questElement.questElementID, Status.Failed);
            }
        }

        if (nextQuestElementIDs.Length != 0)
        {
            SetStatuses(nextQuestElementIDs, Status.Active);
        }
        else QuestManager.SetQuestElementStatus(myElement.questID, Status.Completed);
        return true;
    }

    private bool ParallelConstraintsCheck()
    {
        foreach (QuestElement questElement in questElementIDsFailed)
        {
            if (questElement.status != Status.Completed)
                QuestManager.SetQuestElementStatus(questElement.questElementID, Status.Failed);
        }

        bool allCompleted = true;

        foreach (QuestElement questElement in questElementIDs)
        {
            if (QuestManager.GetElementStatus(questElement.questElementID) != Status.Completed)
                allCompleted = false;
        }

        if (!allCompleted) return false;

        if (nextQuestElementIDs.Length != 0)
        {
            SetStatuses(nextQuestElementIDs, Status.Active);
        }
        else QuestManager.SetQuestElementStatus(myElement.questID, Status.Completed);
        return true;
    }

    public static void SetStatuses(QuestElement[] questElements, Status status)
    {
        foreach (QuestElement questElement in questElements)
        {
            QuestManager.SetQuestElementStatus(questElement.questElementID, status);
        }
    }

    ////Parallel Completion
    //public Dependency(Dependencies dependencies)
    //{
    //    if (dependencies != Dependencies.ParallelCompletion)
    //        throw new ArgumentException();
    //    this.dependencies = dependencies;
    //}

    ////Linear
    //public Dependency(Dependencies dependencies, QuestElement[] nextQuestElementIDs, QuestElement[] previousQuestElementIDsCompleted, QuestElement[] previousQuestElementIDsFailed)
    //{
    //    if (dependencies != Dependencies.Linear)
    //        throw new ArgumentException();
    //    this.dependencies = dependencies;
    //    this.nextQuestElementIDs = nextQuestElementIDs;
    //    this.previousQuestElementIDsCompleted = previousQuestElementIDsCompleted;
    //    this.previousQuestElementIDsFailed = previousQuestElementIDsFailed;
    //}

    //public Dependency(Dependencies dependencies, QuestElement[] nextQuestElementIDs, QuestElement[] previousQuestElementIDsCompleted)
    //{
    //    if (dependencies != Dependencies.Linear)
    //        throw new ArgumentException();
    //    this.dependencies = dependencies;
    //    this.nextQuestElementIDs = nextQuestElementIDs;
    //    this.previousQuestElementIDsCompleted = previousQuestElementIDsCompleted;
    //}

    ////Parallel Constraints
    //public Dependency(Dependencies dependencies, QuestElement[] nextQuestElementIDs, QuestElement[] previousQuestElementIDsCompleted, QuestElement[] previousQuestElementIDsFailed, QuestElement[] questElementIDs)
    //{
    //    if (dependencies != Dependencies.Linear)
    //        throw new ArgumentException();
    //    this.dependencies = dependencies;
    //    this.nextQuestElementIDs = nextQuestElementIDs;
    //    this.previousQuestElementIDsCompleted = previousQuestElementIDsCompleted;
    //    this.previousQuestElementIDsFailed = previousQuestElementIDsFailed;
    //    this.questElementIDs = questElementIDs;
    //}
}