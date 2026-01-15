using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ConversationNode
{
    public int currentIndex = 0;
    public Rect rect;
    public Dialogue dialogue;
    public int chosenDialogueIndex = 0;
    public string verse;
    public int selectedLineIndex;
    public bool isCondition = false;
    public bool isDraggable = true;
    public int numOfConditions = 0;
    public List<ConversationNode> children = new List<ConversationNode>();
    public TextAsset optionsFile = null;
    public List<int> selectedOptions = new List<int>();
    public List<string> optionsText = new List<string>();
    public List<Rect> optionsPositions = new List<Rect>();
}
