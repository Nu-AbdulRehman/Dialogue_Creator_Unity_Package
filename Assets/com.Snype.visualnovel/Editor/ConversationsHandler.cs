using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class ConversationsHandler : EditorWindow
{
    private ConversationsDatabase database;
    private Vector2 scroll;

    [MenuItem("Window/Visual Novel Creator/Conversations Handler")]
    public static void ShowWindow()
    {
        GetWindow<ConversationsHandler>("Conversations Handler");
    }

    private void OnEnable()
    {
        database = AssetDatabase.LoadAssetAtPath<ConversationsDatabase>("Assets/ConversationsDatabase.asset");

        if (database == null)
        {
            database = ScriptableObject.CreateInstance<ConversationsDatabase>();
            AssetDatabase.CreateAsset(database, "Assets/ConversationsDatabase.asset");
            AssetDatabase.SaveAssets();
        }
    }

    private void OnGUI()
    {
        GUILayout.Label("Conversations", EditorStyles.boldLabel);
        DisplayScrollView();
        AddConversationButton();

        if (GUI.changed)
        {
            EditorUtility.SetDirty(database);
            AssetDatabase.SaveAssets();
        }
    }

    void DisplayScrollView()
    {
        scroll = EditorGUILayout.BeginScrollView(scroll);

        for (int i = 0; i < database.conversations.Count; i++)
        {
            EditorGUILayout.BeginVertical("box");
            database.conversations[i].conversationName = EditorGUILayout.TextField("Conversation Name", database.conversations[i].conversationName);
            RemoveConversationButton(i);
            EditorGUILayout.EndVertical();
        }

        EditorGUILayout.EndScrollView();
    }

    void RemoveConversationButton(int index)
    {
        if (GUILayout.Button("Remove"))
        {
            var convo = database.conversations[index];

            database.conversations.RemoveAt(index);

            if (convo != null)
            {
                Object.DestroyImmediate(convo, true);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
        }
    }

    void AddConversationButton()
    {
        if (GUILayout.Button("Add Conversation"))
        {
            var newConversation = CreateInstance<Conversation>();
            newConversation.conversationName = "Unassigned";

            if (database == null)
            {
                database = ScriptableObject.CreateInstance<ConversationsDatabase>();
                AssetDatabase.CreateAsset(database, "Assets/ConversationsDatabase.asset");
                AssetDatabase.SaveAssets();
            }

            database.conversations.Add(newConversation);
            AssetDatabase.AddObjectToAsset(newConversation, database);
        }
    }
}

