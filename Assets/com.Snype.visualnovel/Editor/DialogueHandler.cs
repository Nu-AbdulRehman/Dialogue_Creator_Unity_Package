using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System;

public class DialogueHandler : EditorWindow
{
    private Vector2 scroll;
    private int selectedConvoIndex = 0;
    ConversationsDatabase db;
    CharacterDatabase characterDatabase;

    [MenuItem("Window/Visual Novel Creator/Dialogue Handler")]

    public static void ShowWindow()
    {
        GetWindow<DialogueHandler>("Dialogues Handler");
    }

    private void OnEnable()
    {
        db = AssetDatabase.LoadAssetAtPath<ConversationsDatabase>("Assets/ConversationsDatabase.asset");
        characterDatabase = AssetDatabase.LoadAssetAtPath<CharacterDatabase>("Assets/CharacterDatabase.asset");
    }

    private void OnGUI()
    {
        GUILayout.Label("Dialogues", EditorStyles.boldLabel);

        if (db == null)
        {
            db = AssetDatabase.LoadAssetAtPath<ConversationsDatabase>("Assets/ConversationsDatabase.asset");
            EditorGUILayout.HelpBox("No Conversations Database Found! Please Set It Up Using The Windows Under Window/Visual Novel Creator. If A Database Exists Please Ensure It Is Situated Under The Assets Folder", MessageType.Info);
            return;
        }

        if(characterDatabase == null)
        {
            characterDatabase = AssetDatabase.LoadAssetAtPath<CharacterDatabase>("Assets/CharacterDatabase.asset");
            EditorGUILayout.HelpBox("No Characters Database Found! Please Set It Up Using The Character Handler Window Under Window/Visual Novel Creator. If A Characters Database Exists Please Ensure It Is Situated Under The Assets Folder", MessageType.Info);
            return;
        }

        List<Conversation> convos = DisplayConversationsList();

        if (convos == null)
            return;

        Conversation convo = convos[selectedConvoIndex];
        List<Dialogue> dialogues = convo.dialogues;

        EditorGUILayout.BeginVertical();

        scroll = EditorGUILayout.BeginScrollView(scroll);

        for (int i = 0; i < dialogues.Count; i++)
        {
            EditorGUILayout.BeginVertical("box");

            DisplayDialogueDetails(dialogues[i]);

            DisplayCharacterList(dialogues[i]);

            if (GUILayout.Button("Remove Dialogue"))
            {
                dialogues.Remove(dialogues[i]);
            }

            EditorGUILayout.EndVertical();
            EditorGUILayout.Space();
        }

        EditorGUILayout.EndScrollView();

        AddDialogueButton(dialogues);

        EditorGUILayout.EndVertical();
    }

    void DisplayDialogueDetails(Dialogue dialogue)
    {
        dialogue.dialogueFile = (TextAsset)EditorGUILayout.ObjectField("Dialogue File", dialogue.dialogueFile, typeof(TextAsset), false);
        dialogue.dialogueName = EditorGUILayout.TextField("Dialogue Name", dialogue.dialogueName);
    }

    List<Conversation> DisplayConversationsList()
    {
        List<Conversation> convos = new List<Conversation>();
        foreach (Conversation conv in db.conversations)
        {
            if (conv.conversationName != "Unassigned")
            {
                convos.Add(conv);
            }
        }

        if (convos == null || convos.Count == 0)
        {
            EditorGUILayout.HelpBox("No conversations available.", MessageType.Info);
            return null;
        }

        string[] convoNames = new string[convos.Count];
        for (int j = 0; j < convos.Count; j++)
        {
            convoNames[j] = convos[j].conversationName;
        }

        selectedConvoIndex = EditorGUILayout.Popup("Conversation", selectedConvoIndex, convoNames);
        selectedConvoIndex = Mathf.Clamp(selectedConvoIndex, 0, convos.Count - 1);

        return convos;
    }

    void DisplayCharacterList(Dialogue dialogue)
    {
        List<Character> chars = characterDatabase.charactersList;
        string[] charNames = new string[chars.Count];

        for (int j = 0; j < chars.Count; j++)
        {
            charNames[j] = chars[j].characterName;
        }

        int currentIndex = Mathf.Max(0, chars.IndexOf(dialogue.character));
        int newIndex = EditorGUILayout.Popup("Character", currentIndex, charNames);

        if (newIndex >= 0 && newIndex < chars.Count)
        {
            dialogue.character = chars[newIndex];
        }
    }

    void AddDialogueButton(List<Dialogue> dialogues)
    {
        if (GUILayout.Button("Add Dialogue"))
        {
            dialogues.Add(new Dialogue());
        }
    }
}
