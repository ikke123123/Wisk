using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
