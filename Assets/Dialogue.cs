using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;

public class Dialogue : MonoBehaviour
{
    private static Dialogue d = null;

    [SerializeField] private TextMeshProUGUI title = null;
    [SerializeField] private TextMeshProUGUI text = null;
    [SerializeField, Tooltip("The box the dialogue stuff comes in, excluding this script.")] private GameObject container = null;

    private Queue<DialogueBlock> dialogueBlock = new Queue<DialogueBlock>();

    private Coroutine displayCoroutine = null;

    private const float timeBetweenText = 1;

    private void Awake()
    {
        if (d != null && d != this)
            Destroy(d);
        d = this;
    }

    private void Start()
    {
        container.SetActive(false);
    }

    public static void AddText(string title, string text)
    {
        AddText(new DialogueBlock(title, text));
    }

    public static void AddText(DialogueBlock dialogueBlock)
    {
        d.dialogueBlock.Enqueue(dialogueBlock);
        if (d.displayCoroutine != null)
            d.displayCoroutine = d.StartCoroutine(d.TextLoop());
    }

    public static void AddText(DialogueBlock[] dialogueBlocks)
    {
        foreach (DialogueBlock dialogueBlock in dialogueBlocks)
            AddText(dialogueBlock);
    }

    private IEnumerator TextLoop()
    {
        while (dialogueBlock.Count != 0)
        {
            DialogueBlock tempBlock = dialogueBlock.Peek();
            container.SetActive(true);
            title.text = tempBlock.title;
            text.text = "";
            StartCoroutine(WordAnimator(tempBlock));
            yield return new WaitForSeconds(tempBlock.totalTime);
            container.SetActive(false);
            dialogueBlock.Dequeue();
            yield return new WaitForSeconds(timeBetweenText);
        }
        displayCoroutine = null;
    }

    private IEnumerator WordAnimator(DialogueBlock dialogueBlock)
    {
        foreach (Word word in dialogueBlock.words)
        {
            text.text += word;
            yield return new WaitForSeconds(word.showTime);
        }
    }
}