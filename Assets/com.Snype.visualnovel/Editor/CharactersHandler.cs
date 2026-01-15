using NUnit.Framework;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class CharactersHandler : EditorWindow
{
    CharacterDatabase characterDatabase;
    private Vector2 scroll;

    [MenuItem("Window/Visual Novel Creator/Characters Handler")]

    public static void ShowWindow()
    {
        GetWindow<CharactersHandler>("Characters Handler");
    }

    private void OnEnable()
    {
        characterDatabase = AssetDatabase.LoadAssetAtPath<CharacterDatabase>("Assets/CharacterDatabase.asset");

        if (characterDatabase == null)
        {
            characterDatabase = ScriptableObject.CreateInstance<CharacterDatabase>();
            AssetDatabase.CreateAsset(characterDatabase, "Assets/CharacterDatabase.asset");
            AssetDatabase.SaveAssets();
        }
    }

    private void OnGUI()
    {
        GUILayout.Label("Characters", EditorStyles.boldLabel);
        DisplayScrollView();
        AddCharacterButton();
        EditorGUILayout.EndVertical();
    }

    void DisplayScrollView()
    {
        EditorGUILayout.BeginVertical();
        scroll = EditorGUILayout.BeginScrollView(scroll);
        DisplayCharacterList();
        EditorGUILayout.EndScrollView();
    }

    void DisplayCharacterList()
    {
        for (int i = 0; i < characterDatabase.charactersList.Count; i++)
        {
            characterDatabase.charactersList[i].characterName = EditorGUILayout.TextField("Character Name", characterDatabase.charactersList[i].characterName);
            characterDatabase.charactersList[i].characterSprite = (Sprite)EditorGUILayout.ObjectField("Sprite", characterDatabase.charactersList[i].characterSprite, typeof(Sprite), false);

            RemoveCharacterButton(i);
        }
    }

    void RemoveCharacterButton(int index)
    {
        if (GUILayout.Button("Remove"))
        {
            Character character = characterDatabase.charactersList[index];
            characterDatabase.charactersList.RemoveAt(index);

            if (character != null)
            {
                Object.DestroyImmediate(character, true);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
        }
    }

    void AddCharacterButton()
    {
        if (GUILayout.Button("Add Character"))
        {
            if (characterDatabase == null)
            {
                characterDatabase = ScriptableObject.CreateInstance<CharacterDatabase>();
                AssetDatabase.CreateAsset(characterDatabase, "Assets/CharacterDatabase.asset");
                AssetDatabase.SaveAssets();
            }

            Character character = CreateInstance<Character>();

            characterDatabase.charactersList.Add(character);
            AssetDatabase.AddObjectToAsset(character, characterDatabase);
        }
    }
}
