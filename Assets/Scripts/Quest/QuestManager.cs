using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuestManager : MonoBehaviour
{
    public static QuestManager qM = null;

    [SerializeField] private Quest[] quests = null;

    [Header("Debug")]
    [SerializeField] private bool save = true;
    [SerializeField] private List<Quest> activeQuests = new List<Quest>();
    [SerializeField] private List<Quest> unlockedQuests = new List<Quest>();
    [SerializeField] private List<Quest> lockedQuests = new List<Quest>();
    [SerializeField] private List<Quest> completedQuests = new List<Quest>();

    [SerializeField] private List<QuestElement> activeElements = new List<QuestElement>();

    private Dictionary<string, Quest> idToQuest = new Dictionary<string, Quest>();
    private Dictionary<string, QuestElement> idToQuestElement = new Dictionary<string, QuestElement>();

    [Header("Test")]
    [SerializeField] private string activeQuest;
    [SerializeField] private bool activateActiveQuest;


    private void Awake()
    {
        if (qM != null && qM != this)
        {
            Destroy(qM);
        }
        qM = this;
    }

    private void Start()
    {
        foreach (Quest quest in quests)
        {
            idToQuest.Add(quest.questID, quest);

            quest.CheckStatus(save);

            AddRemoveFromStatusLists(quest, true);

            foreach (QuestElement questElement in quest.questElements)
            {
                idToQuestElement.Add(questElement.questElementID, questElement);
                questElement.CheckStatus(save);
            }
        }
    }

    private void Update()
    {
        if (activateActiveQuest)
            SetQuestStatus(activeQuest, Status.Active);
    }

    public static void SetQuestStatus(string questID, Status status)
    {
        Quest quest = GetQuest(questID);

        if (status != quest.status)
        {
            qM.AddRemoveFromStatusLists(quest, false);

            quest.SetStatus(status, qM.save);

            qM.AddRemoveFromStatusLists(quest, true);
        }
    }

    public static void SetQuestElementStatus(string elementID, Status status)
    {
        QuestElement questElement = GetQuestElement(elementID);

        if (status != questElement.status)
        {
            questElement.SetStatus(status);
        }
    }

    public static Quest GetQuest(string questID)
    {
        if (qM == null)
            throw new NullReferenceException();
        if (!qM.idToQuest.ContainsKey(questID))
            throw new ArgumentException();

        return qM.idToQuest[questID];
    }

    public static QuestElement GetQuestElement(string elementID)
    {
        if (qM == null)
            throw new NullReferenceException();
        if (!qM.idToQuestElement.ContainsKey(elementID))
            throw new ArgumentException();

        return qM.idToQuestElement[elementID];
    }

    public static Status GetQuestStatus(string questID) => GetQuest(questID).status;

    public static Status GetElementStatus(string elementID) => GetQuestElement(elementID).status;

    private void AddRemoveFromStatusLists(Quest quest, bool add)
    {
        int questStatus = (int)quest.status;
        if (add)
        {
            switch (questStatus)
            {
                case 0: //Active
                    activeQuests.Add(quest);
                    break;
                case 1: //Unlocked
                    unlockedQuests.Add(quest);
                    break;
                case 2: //Locked
                    lockedQuests.Add(quest);
                    break;
                case 3: //Completed
                    completedQuests.Add(quest);
                    break;
            }
        }
        else
        {
            switch (questStatus)
            {
                case 0: //Active
                    activeQuests.Remove(quest);
                    break;
                case 1: //Unlocked
                    unlockedQuests.Remove(quest);
                    break;
                case 2: //Locked
                    lockedQuests.Remove(quest);
                    break;
                case 3: //Completed
                    completedQuests.Remove(quest);
                    break;
            }
        }
    }
}
