using UnityEngine;

[System.Serializable]
public struct BasicDialogue
{
    public BasicDialogue(string title, string text)
    {
        this.title = title;
        this.text = text;
    }
    public string title;
    [TextArea] public string text;
}
