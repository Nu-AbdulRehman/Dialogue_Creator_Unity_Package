using System.Collections.Generic;
using UnityEngine;

public class Conversation : ScriptableObject
{
    public string conversationName = "Unassigned";
    public List<Dialogue> dialogues = new List<Dialogue>();
    public List<ConversationNode> nodesList = new List<ConversationNode>();
}
