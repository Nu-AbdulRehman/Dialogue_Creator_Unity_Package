using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ConversationsDatabase", menuName = "Visual Novel/Conversations Database")]
public class ConversationsDatabase : ScriptableObject
{
    public List<Conversation> conversations = new List<Conversation>();
}
