using System.Collections.Generic;
using UnityEngine;

public enum ConversationNodeType
{
    Dialogue,
    Condition,
    RunFunction
}

[System.Serializable]
public class ConversationNode
{
    public ConversationNodeType nodeType = ConversationNodeType.Dialogue;

    public int currentIndex = 0;
    public Rect rect;
    public int id;
    public List<int> childIds = new();

    // Dialogue NODE
    public Dialogue dialogue;
    public int chosenDialogueIndex = 0;
    public string verse;
    public int selectedLineIndex;

    // Conditional NODE
    public int numOfConditions = 0;
    public List<int> selectedOptions = new();
    public List<string> optionsText = new();
    public List<Rect> optionsPositions = new();
    public TextAsset optionsFile;

    // Functional NODE
    [System.NonSerialized] public MonoBehaviour editorTargetScript;
    public string targetScriptTypeName;
    public string methodName;
    public List<FunctionParameter> parameters = new();

    // All NODES
    public bool isDraggable = true;
    [System.NonSerialized]
    public List<ConversationNode> children = new();
}

// Functional NODE
[System.Serializable]
public class FunctionParameter
{
    public string name;
    public string typeName;

    public int intValue;
    public float floatValue;
    public string stringValue;
    public bool boolValue;
    public UnityEngine.Object objectValue;
}