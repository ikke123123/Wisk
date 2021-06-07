using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using TMPro;

public class MessageManager : MonoBehaviour
{
    public static MessageManager mM = null;

    [SerializeField] private GameObject achievement = null;
    [SerializeField] private GameObject inventory = null;
    [SerializeField] private GameObject other = null;
    [SerializeField] private GameObject quest = null;

    private void Awake()
    {
        if (mM != null && mM != this)
        {
            Destroy(mM);
        }
        mM = this;
    }



    public static void SendMessage(string top, string bottom)
    {
        if (mM == null)
            throw new NullReferenceException();
        mM.Message(TypeOf.Other, top, bottom);
    }

    public static void SendMessage(TypeOf typeOf, string bottom)
    {
        if (mM == null)
            throw new NullReferenceException();
        mM.Message(typeOf, bottom);
    }

    public static void SendMessage(TypeOf typeOf, string top, string bottom)
    {
        if (mM == null)
            throw new NullReferenceException();
        mM.Message(typeOf, top, bottom);
    }



    private void Message(TypeOf typeOf, string top, string bottom)
    {
        GameObject tempGameObject = null;

        switch ((int)typeOf)
        {
            case 2:
                tempGameObject = other;
                break;
            case 3:
                tempGameObject = quest;
                break;
            default:
                throw new ArgumentException();
        }

        tempGameObject = Instantiate(tempGameObject, transform);

        switch ((int)typeOf)
        {
            case 2:
            case 3:
                tempGameObject.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = bottom;
                tempGameObject.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = top;
                break;
        }
    }

    private void Message(TypeOf typeOf, string bottom)
    {
        GameObject tempGameObject = null;

        switch ((int)typeOf)
        {
            case 0:
                tempGameObject = achievement;
                break;
            case 1:
                tempGameObject = inventory;
                break;
            default:
                throw new ArgumentException();
        }

        tempGameObject = Instantiate(tempGameObject, transform);

        switch ((int)typeOf)
        {
            case 1:
            case 2:
                tempGameObject.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = bottom;
                break;
        }
    }

    public enum TypeOf { Achievement, Inventory, Other, Quest };
}
