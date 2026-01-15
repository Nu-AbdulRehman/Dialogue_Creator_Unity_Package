using UnityEngine;

[System.Serializable]
public class Dialogue
{
    [Header("Name")]
    public string dialogueName = "";

    [Header("Text File")]
    public TextAsset dialogueFile;

    [Header("Character")]
    public Character character;
}
