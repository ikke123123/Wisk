using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct Word
{
    public Word(string word)
    {
        this.word = word;
        characters = word.Length;
        showTime = characters * characterTime;
    }
    public string word;
    public int characters;
    public float showTime;

    public const float characterTime = 0.1f;
}
