using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class QuestUIManager : MonoBehaviour
{
    public static List<QuestUIManager> qUIM = new List<QuestUIManager>();

    [SerializeField] private TextMeshProUGUI quest = null;
    [SerializeField] private TextMeshProUGUI[] questElements = null;

    [Header("Debug")]
    public bool active = false;
    public bool[] activeSpots = new bool[] { false, false, false };

    private void Awake()
    {
        if (qUIM == null) qUIM = new List<QuestUIManager>();
        if (!qUIM.Contains(this)) qUIM.Add(this);
    }

    public static QuestUIManager GetUIManager()
    {
        foreach (QuestUIManager questUIManager in qUIM)
        {
            if (questUIManager.active == false)
            {
                questUIManager.Enable(true);
                return questUIManager;
            }
        }
        throw new NullReferenceException();
    }

    public void ReleaseUIManager()
    {
        active = false;
        Enable(false);
    }

    public int GetElementSpot()
    {
        for (int i = 0; i < activeSpots.Length; i++)
        {
            if (!activeSpots[i])
            {
                questElements[i].gameObject.SetActive(true);
                activeSpots[i] = true;
                return i;
            }
        }
        throw new NullReferenceException();
    }

    public void ReleaseElementSpot(int index)
    {
        questElements[index].gameObject.SetActive(false);
        activeSpots[index] = false;
    }

    //public void SetText(string quest, string[] questElements)
    //{
    //    SetQuest(quest);
    //    SetQuestElements(questElements);
    //}

    public void SetQuest(string quest)
    {
        this.quest.text = quest;
    }

    public void SetQuestElement(string questElement, int index)
    {
        questElements[index].text = questElement;
    }


    //private void SetQuestElements(string[] questElements)
    //{
    //    for (int i = 0; i < this.questElements.Length; i++)
    //    {
    //        if (i < questElements.Length)
    //        {
    //            this.questElements[i].gameObject.SetActive(true);
    //            this.questElements[i].text = questElements[i];
    //        }
    //        else this.questElements[i].gameObject.SetActive(false);
    //    }
    //}

    public void Enable(bool enable = true)
    {
        gameObject.SetActive(enable);
    }


}
