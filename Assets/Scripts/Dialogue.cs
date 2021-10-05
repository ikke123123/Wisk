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

    private Queue<DialogueBlock> dialogueBlockQueue = new Queue<DialogueBlock>();

    private Coroutine displayCoroutine = null;

    //private const float timeBetweenText = 0;

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
        d.PlayText(dialogueBlock);
    }

    public static void AddText(DialogueBlock[] dialogueBlocks)
    {
        foreach (DialogueBlock dialogueBlock in dialogueBlocks)
            AddText(dialogueBlock);
    }

    private void PlayText(DialogueBlock dialogueBlock)
    {
        dialogueBlockQueue.Enqueue(dialogueBlock);
        if (displayCoroutine == null)
            displayCoroutine = StartCoroutine(TextLoop());
    }

    private IEnumerator TextLoop()
    {
        while (dialogueBlockQueue.Count != 0)
        {
            DialogueBlock tempBlock = dialogueBlockQueue.Peek();
            container.SetActive(true);
            title.text = tempBlock.title;
            text.text = "";
            StartCoroutine(WordAnimator(tempBlock));
            yield return new WaitForSeconds(tempBlock.totalTime);
            container.SetActive(false);
            dialogueBlockQueue.Dequeue();
            //yield return new WaitForSeconds(timeBetweenText);
        }
        displayCoroutine = null;
    }

    private IEnumerator WordAnimator(DialogueBlock dialogueBlock)
    {
        foreach (Word word in dialogueBlock.words)
        {
            StartCoroutine(CharacterAnimator(word));
            yield return new WaitForSeconds(word.showTime);
            text.text += " ";
        }
    }

    private IEnumerator CharacterAnimator(Word word)
    {
        foreach (char character in word.word.ToCharArray())
        {
            text.text += character;
            yield return new WaitForSeconds(0.05f);
        }
    }
}