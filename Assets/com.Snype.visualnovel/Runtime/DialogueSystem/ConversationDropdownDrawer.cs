using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

public class ConversationDropdownAttribute : PropertyAttribute { }

[CustomPropertyDrawer(typeof(ConversationDropdownAttribute))]
public class ConversationDropdownDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        var db = AssetDatabase.LoadAssetAtPath<ConversationsDatabase>("Assets/ConversationsDatabase.asset");
        if (db == null)
        {
            EditorGUI.LabelField(position, "No Conversations Database found!");
            return;
        }

        List<string> names = new List<string>();
        foreach (var c in db.conversations)
        {
            if(c .conversationName != "Unassigned")
            {
                names.Add(c != null ? c.conversationName : "NULL");
            }
        }

        int currentIndex = db.conversations.IndexOf(property.objectReferenceValue as Conversation);

        int newIndex = EditorGUI.Popup(position, label.text, currentIndex, names.ToArray());

        if (newIndex >= 0 && newIndex < db.conversations.Count)
        {
            property.objectReferenceValue = db.conversations[newIndex];
        }
    }
}
