using UnityEngine;

[System.Serializable]
public class Character : ScriptableObject
{
    [Header("Character Details")]
    public string characterName;

    [Header("Visuals")]
    public Sprite characterSprite;
}
