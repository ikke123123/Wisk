using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public struct DialogueBlock
{
    public DialogueBlock(string title, string text)
    {
        List<Word> words = new List<Word>();
        totalTime = 0;
        foreach (string word in text.Split(' '))
        {
            Word newWord = new Word(word);
            words.Add(new Word(word));
            totalTime += newWord.showTime;
        }
        this.words = words.ToArray();
        this.title = title;
        totalTime += endTime;
    }

    public string title;
    public Word[] words;
    public float totalTime;

    public const float endTime = 1f;
}
