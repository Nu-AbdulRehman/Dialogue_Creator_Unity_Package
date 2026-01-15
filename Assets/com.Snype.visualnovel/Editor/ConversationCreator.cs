using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

public class ConversationCreator : EditorWindow
{
    private int selectedConvoIndex = 0;
    private Vector2 drag;
    private Vector2 scroll;
    private Rect canvas;
    public int currentConditionsAmount = 0;
    public float windowScale = 1f;

    private List<ConversationNode> nodes;

    ConversationsDatabase db;

    [MenuItem("Window/Visual Novel Creator/Conversation Creator")]

    public static void ShowWindow()
    {
        GetWindow<ConversationCreator>("Conversation Creator");
    }

    private void OnEnable()
    {
        db = AssetDatabase.LoadAssetAtPath<ConversationsDatabase>("Assets/ConversationsDatabase.asset");
    }

    private void OnGUI()
    {
        GUILayout.Label("Conversation Creator", EditorStyles.boldLabel);

        if(db == null)
        {
            db = AssetDatabase.LoadAssetAtPath<ConversationsDatabase>("Assets/ConversationsDatabase.asset");
            EditorGUILayout.HelpBox("No Conversations Database Found! Please Set It Up Using The Windows Under Window/Visual Novel Creator. If A Database Exists Please Ensure It Is Situated Under The Assets Folder", MessageType.Info);
            return;
        }

        List<Conversation> convos = new List<Conversation>();
        foreach (Conversation conv in db.conversations)
        {
            if (conv.conversationName != "Unassigned" && conv.conversationName != "")
            {
                convos.Add(conv);
            }
        }

        if (convos == null || convos.Count == 0)
        {
            EditorGUILayout.HelpBox("No Conversations Available.", MessageType.Info);
            return;
        }

        string[] convoNames = new string[convos.Count];
        for (int i = 0; i < convos.Count; i++)
        {
            convoNames[i] = convos[i].conversationName;
        }

        if(convos.Count <= selectedConvoIndex)
        {
            selectedConvoIndex = 0;
        }
        selectedConvoIndex = EditorGUILayout.Popup("Conversation", selectedConvoIndex, convoNames);
        Conversation convo = convos[selectedConvoIndex];
        nodes = convo.nodesList;
        RebuildChildrenFromIds();
        GUILayout.Space(10);

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Add Node"))
        {
            nodes.Add(new ConversationNode()
            {
                currentIndex = 0,
                id = GetNextNodeId(),
                rect = new Rect(100 + scroll.x, 100 + scroll.y, 200, 160),
                dialogue = convo.dialogues.Count > 0 ? convo.dialogues[0] : null
            });
        }

        if (GUILayout.Button("Add Conditional Node"))
        {
            ConditionalPromptWindow.Show(convo, (c) =>
            {
                AddConditionalNode(c, scroll);
            });
        }

        if (GUILayout.Button("Add Run Function Node"))
        {
            nodes.Add(new ConversationNode
            {
                id = GetNextNodeId(),
                nodeType = ConversationNodeType.RunFunction,
                rect = new Rect(100 + scroll.x, 100 + scroll.y, 220, 120)
            });
        }

        GUILayout.EndHorizontal();

        GUILayout.Space(10);

        canvas = CalculateCanvas(nodes);

        HandleZooming();

        scroll = EditorGUILayout.BeginScrollView(scroll, true, true, GUILayout.ExpandHeight(true));
        {
            GUILayoutUtility.GetRect(canvas.width, canvas.height);
            GUI.BeginGroup(canvas);
            DrawNodes(convo);
            GUI.EndGroup();
            ProcessEvents(Event.current);
        }
        EditorGUILayout.EndScrollView();

        OrderNodes(nodes);

        SyncChildIds();

        if (GUI.changed)
        {
            Undo.RecordObject(convo, "Modify Conversation");
            EditorUtility.SetDirty(convo);
            EditorUtility.SetDirty(db);
            Repaint();
        }
    }

    void AddConditionalNode(Conversation convo, Vector2 scroll)
    {
        int currentOptions = PlayerPrefs.GetInt("ConditionsOption", 2);
        nodes.Add(new ConversationNode()
        {
            numOfConditions = currentOptions,
            nodeType = ConversationNodeType.Condition,
            currentIndex = 0,
            id = GetNextNodeId(),
            rect = new Rect(100 + scroll.x, 100 + scroll.y, 200 * currentOptions, 120),
            dialogue = convo.dialogues.Count > 0 ? convo.dialogues[0] : null
        });
    }

    private void DrawNodes(Conversation convo)
    {
        for (int i = 0; i < nodes.Count; i++)
        {
            ConversationNode node = nodes[i];

            switch (node.nodeType)
            {
                case ConversationNodeType.Dialogue:
                    DrawNode(node, convo, i);
                    break;

                case ConversationNodeType.Condition:
                    DrawConditionalNode(node, i);
                    DragConditionalOptions(node);
                    break;

                case ConversationNodeType.RunFunction:
                    DrawRunFunctionNode(node, i);
                    break;
            }

            if (Event.current.type == EventType.MouseDrag && node.rect.Contains(Event.current.mousePosition))
            {
                if(node.isDraggable)
                {
                    node.rect.position += Event.current.delta;
                    DragChildren(node, Event.current.delta);
                    GUI.changed = true;
                }
            }
        }
    }

    private void DrawConditionalNode(ConversationNode node, int i)
    {
        GUI.Box(node.rect, "", EditorStyles.helpBox);
        GUI.BeginGroup(node.rect);

        EditorGUI.LabelField(new Rect(node.rect.width / 2 - 50, 5, node.rect.width - 20, 20), "Conditional Node");

        if (GUI.Button(new Rect(10, 22.5f, node.rect.width - 20, 20), "Remove Node"))
        {
            nodes.RemoveAt(i);
        }

        node.isDraggable = EditorGUI.Toggle(new Rect(10, 42.5f, node.rect.width - 40, 20), "Is Draggable?", node.isDraggable);

        node.optionsFile = (TextAsset)EditorGUI.ObjectField(new Rect(10, 62.5f, node.rect.width - 40, 20), "Options File", node.optionsFile, typeof(TextAsset), false);

        CheckandConnectConditionalNode(node);

        GUI.EndGroup();

        if (node.optionsPositions.Count != node.numOfConditions)
        {
            node.optionsPositions.Clear();
            for (int j = 0; j < node.numOfConditions; j++)
            {
                node.optionsPositions.Add(new Rect(j * 200f, 70f, node.rect.width / node.numOfConditions, 60f));
            }
        }

        for (int j = 0; j < node.numOfConditions; j++)
        {
            Rect box = node.optionsPositions[j];

            Vector2 startTangent = node.rect.center + Vector2.right * 50f;
            Vector2 endTangent = box.center + Vector2.left * 50f;

            Handles.DrawBezier(new Vector2(node.rect.center.x, node.rect.center.y + 30f), box.center, startTangent, endTangent, Color.black, null, 2.5f);

            GUI.Box(box, "", EditorStyles.helpBox);
            EditorGUI.LabelField(
                new Rect(box.x + box.width / 3f + 5f, (box.y + box.height / 6f) - 7.5f, 100f, 25f),
                (node.children.Count > j) ? node.children[j].currentIndex.ToString() : "Choice " + (j + 1),
                EditorStyles.boldLabel
            );

            if(node.selectedOptions.Count < j + 1)
                node.selectedOptions.Add(0);

            DisplayOptionsVerseDropDown(node, node.optionsPositions[j], j);
        }
    }

    void DisplayOptionsVerseDropDown(ConversationNode node, Rect optionsPos, int index)
    {
        if (node.optionsFile != null)
        {
            string[] lines = node.optionsFile.text.Split(
                new[] { '\r', '\n' }, System.StringSplitOptions.RemoveEmptyEntries);

            if (lines.Length > 0)
            {
                EditorGUI.LabelField(new Rect(optionsPos.x + 10, optionsPos.y + 17.5f, optionsPos.width - 20, 20), "Verse", EditorStyles.boldLabel);

                if (node.selectedOptions[index] >= lines.Length)
                    node.selectedOptions[index] = 0;

                node.selectedOptions[index] = EditorGUI.Popup(
                    new Rect(optionsPos.x + 10, optionsPos.y + 35, optionsPos.width - 20, 20),
                    node.selectedOptions[index],
                    lines
                );

                node.optionsText.Clear();

                foreach (int i in node.selectedOptions)
                {
                    node.optionsText.Add(lines[i]);
                }
            }
            else
            {
                EditorGUI.LabelField(new Rect(optionsPos.x + 30, optionsPos.y + 25, optionsPos.width - 20, 20), "No lines found", EditorStyles.boldLabel);
            }
        }
        else
        {
            EditorGUI.LabelField(new Rect(optionsPos.x + 30, optionsPos.y + 25, optionsPos.width - 20, 20), "No Options File found!", EditorStyles.boldLabel);
        }
    }


    public void DragConditionalOptions(ConversationNode node)
    {
        if (!(node.nodeType == ConversationNodeType.Condition))
            return;

        if (!node.isDraggable)
            return;

        for(int i = 0; i < node.numOfConditions; i++)
        {
            if (Event.current.type == EventType.MouseDrag && node.optionsPositions[i].Contains(Event.current.mousePosition))
            {
                Rect r = node.optionsPositions[i];
                r.position += Event.current.delta;
                node.optionsPositions[i] = r;
                DragChildren(node, Event.current.delta, true, i);
                GUI.changed = true;
            }
        }
    }

    public void DragChildren(ConversationNode node, Vector2 offset, bool isCondition = false, int childIndex = -1)
    {
        if(!isCondition)
        {
            if (node.nodeType == ConversationNodeType.Condition)
                return;
        }

        if (node.children.Count == 0)
            return;

        if(childIndex == -1)
        {
            foreach (ConversationNode childNode in node.children)
            {
                childNode.rect.position += offset;
                DragChildren(childNode, offset, isCondition);
            }
        }
        else
        {
            int count = node.children.Count;
            for (int i = 0; i < count; i++)
            {
                if (i == childIndex)
                {
                    node.children[i].rect.position += offset;
                    DragChildren(node.children[i], offset, isCondition);
                }
            }
        }

    }

    private void DrawNode(ConversationNode node, Conversation convo, int i)
    {
        if(windowScale == 0)
            windowScale = 1;

        Rect box = new Rect(node.rect.position.x, node.rect.position.y, node.rect.width * windowScale, node.rect.height * windowScale);

        GUI.Box(box, "", EditorStyles.helpBox);

        GUI.BeginGroup(box);

        EditorGUI.LabelField(new Rect(10, 5, box.width - 20, 20), "Dialogue_Index", (node.currentIndex.ToString() == "0") ? "Play" : node.currentIndex.ToString());

        if (node.children.Count > 0)
        {
            EditorGUI.LabelField(new Rect(10, 17.5f, box.width - 20, 20), "Connected_Index", node.children[0].currentIndex.ToString());
        }
        else
        {
            EditorGUI.LabelField(new Rect(10, 17.5f, box.width - 20, 20), "Connected_Index", "N/A");
        }

        string[] dialogueNames = new string[convo.dialogues.Count];
        for (int j = 0; j < convo.dialogues.Count; j++)
        {
            dialogueNames[j] = convo.dialogues[j].dialogueName != "" ? convo.dialogues[j].dialogueName : "Unassigned";
        }

        EditorGUI.LabelField(new Rect(10, 30, box.width - 20, 20), "Dialogue");
        int currentIndex = convo.dialogues.IndexOf(node.dialogue);

        if (currentIndex == -1 && node.chosenDialogueIndex >= 0 && node.chosenDialogueIndex < convo.dialogues.Count)
        {
            currentIndex = node.chosenDialogueIndex;
            node.dialogue = convo.dialogues[currentIndex];
        }

        int newIndex = EditorGUI.Popup(
        new Rect(10, 50, box.width - 20, 20),
        currentIndex,
        dialogueNames
        );

        if (newIndex != currentIndex && newIndex >= 0 && newIndex < convo.dialogues.Count)
        {
            Undo.RecordObject(convo, "Change Dialogue");
            node.chosenDialogueIndex = newIndex;
            node.dialogue = convo.dialogues[newIndex];
            EditorUtility.SetDirty(convo);
        }

        if (node.dialogue != null && node.dialogue.dialogueFile != null && !string.IsNullOrEmpty(node.dialogue.dialogueName))
        {

            string[] lines = node.dialogue.dialogueFile.text.Split(
                new[] { '\r', '\n' }, System.StringSplitOptions.RemoveEmptyEntries);

            if (lines.Length > 0)
            {
                EditorGUI.LabelField(new Rect(10, 70, box.width - 20, 20), "Verse");

                if (node.selectedLineIndex >= lines.Length)
                    node.selectedLineIndex = 0;

                node.selectedLineIndex = EditorGUI.Popup(
                    new Rect(10, 90, box.width - 20, 20),
                    node.selectedLineIndex,
                    lines
                );

                node.verse = lines[node.selectedLineIndex];
            }
            else
            {
                EditorGUI.LabelField(new Rect(10, 70, box.width - 20, 20), "No lines found");
            }
        }
        else
        {
            EditorGUI.LabelField(new Rect(10, 70, box.width - 20, 20), "No Dialogue File found!");
        }

        if (GUI.Button(new Rect(10, 110, box.width - 20, 20), "Remove Node"))
        {
            nodes.RemoveAt(i);
        }

        node.isDraggable = EditorGUI.Toggle(new Rect(10, 130, box.width - 40, 20), "Is Draggable?", node.isDraggable);

        CheckandConnectNodes(node);

        GUI.EndGroup();
    }


    public bool IsSupportedParameter(System.Type t)
    {
        return t == typeof(int) || t == typeof(float) || t == typeof(string) || t == typeof(bool) || typeof(UnityEngine.Object).IsAssignableFrom(t);
    }

    void SyncParameters(ConversationNode node, MethodInfo method)
    {
        var methodParams = method.GetParameters();

        bool rebuild = node.parameters.Count != methodParams.Length;

        if (!rebuild)
        {
            for (int i = 0; i < methodParams.Length; i++)
            {
                if (node.parameters[i].typeName != methodParams[i].ParameterType.AssemblyQualifiedName)
                {
                    rebuild = true;
                    break;
                }
            }
        }

        if (!rebuild)
            return;

        node.parameters.Clear();

        foreach (var param in methodParams)
        {
            FunctionParameter fp = new FunctionParameter
            {
                name = param.Name,
                typeName = param.ParameterType.AssemblyQualifiedName
            };

            node.parameters.Add(fp);
        }
    }

    void DrawFunctionParameters(ConversationNode node, ref float y)
    {
        if (node.parameters == null || node.parameters.Count == 0)
            return;

        EditorGUI.LabelField(
            new Rect(10, y, node.rect.width - 20, 18),
            "Parameters",
            EditorStyles.boldLabel
        );

        y += 20f;

        foreach (var param in node.parameters)
        {
            var type = System.Type.GetType(param.typeName);
            if (type == null)
                continue;

            Rect fieldRect = new Rect(10, y, node.rect.width - 20, 18);

            if (type == typeof(int))
            {
                param.intValue = EditorGUI.IntField(fieldRect, param.name, param.intValue);
            }
            else if (type == typeof(float))
            {
                param.floatValue = EditorGUI.FloatField(fieldRect, param.name, param.floatValue);
            }
            else if (type == typeof(string))
            {
                param.stringValue = EditorGUI.TextField(fieldRect, param.name, param.stringValue);
            }
            else if (type == typeof(bool))
            {
                param.boolValue = EditorGUI.Toggle(fieldRect, param.name, param.boolValue);
            }
            else if (typeof(UnityEngine.Object).IsAssignableFrom(type))
            {
                param.objectValue = EditorGUI.ObjectField(
                    fieldRect,
                    param.name,
                    param.objectValue,
                    type,
                    true
                );
            }

            y += 22f;
        }
    }

    void RestoreEditorTarget(ConversationNode node)
    {
    #if UNITY_EDITOR
        if (node.editorTargetScript != null)
            return;

        if (string.IsNullOrEmpty(node.targetScriptTypeName))
            return;

        MonoBehaviour[] allScripts = GameObject.FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None);

        foreach (var script in allScripts)
        {
            if (script.GetType().Name == node.targetScriptTypeName)
            {
                node.editorTargetScript = script;
                break;
            }
        }
     #endif
    }

    void DrawRunFunctionNode(ConversationNode node, int i)
    {
        RestoreEditorTarget(node);

        GUI.Box(node.rect, "", EditorStyles.helpBox);
        GUI.BeginGroup(node.rect);

        EditorGUI.LabelField(
            new Rect(10, 5, node.rect.width - 20, 20),
            "Run Function Node",
            EditorStyles.boldLabel
        );

        node.editorTargetScript = (MonoBehaviour)EditorGUI.ObjectField(
            new Rect(10, 30, node.rect.width - 20, 18),
            "Target Script",
            node.editorTargetScript,
            typeof(MonoBehaviour),
            true
        );

        if (node.editorTargetScript != null)
        {
            node.targetScriptTypeName = node.editorTargetScript.GetType().Name;
        }

        float y = 55f;

        if (node.editorTargetScript != null)
        {
            MethodInfo[] methods = node.editorTargetScript.GetType()
                .GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .Where(m =>
                    m.ReturnType == typeof(void) &&
                    m.GetParameters().All(p => IsSupportedParameter(p.ParameterType))
                )
                .ToArray();

            if (methods.Length > 0)
            {
                string[] methodNames = methods.Select(m => m.Name).ToArray();

                int index = Mathf.Max(0, System.Array.IndexOf(methodNames, node.methodName));

                int newIndex = EditorGUI.Popup(
                    new Rect(10, y, node.rect.width - 20, 18),
                    index,
                    methodNames
                );

                if (newIndex != index)
                {
                    node.methodName = methodNames[newIndex];
                    SyncParameters(node, methods[newIndex]);
                }

                y += 22f;

                DrawFunctionParameters(node, ref y);
            }
            else
            {
                EditorGUI.LabelField(
                    new Rect(10, y, node.rect.width - 20, 18),
                    "No Valid Void Methods Found"
                );
            }
        }

        if (GUI.Button(new Rect(10, node.rect.height - 25, node.rect.width - 20, 20), "Remove Node"))
            nodes.RemoveAt(i);

        node.rect.height = Mathf.Max(120, y + 30);

        GUI.EndGroup();

        CheckandConnectNodes(node);
    }

    private void OrderNodes(List<ConversationNode> convo)
    {
        for (int i = 0; i < convo.Count; i++)
        {
            for (int j = i; j < convo.Count; j++)
            {
                if (convo[i].rect.position.y > convo[j].rect.position.y)
                {
                    ConversationNode tempNode = convo[i];
                    convo[i] = convo[j];
                    convo[j] = tempNode;
                }
            }
        }

        for (int i = 0; i < convo.Count; i++)
        {
            convo[i].currentIndex = i;
        }
    }

    public void CheckandConnectNodes(ConversationNode node)
    {
        if (node.nodeType == ConversationNodeType.Condition)
            return;

        bool connectionIsMade = false;

        if (nodes.Count < 2)
        {
            node.children.Clear();
            return;
        }

        ConversationNode currentClosest = null;
        foreach (ConversationNode nod in nodes)
        {
            if (nod != node && isInXBounds(nod.rect.center, node.rect.center))
            {
                if (currentClosest == null)
                {
                    currentClosest = nod;
                    connectionIsMade = true;
                }
                else if (isInIndexBounds(node.currentIndex, nod.currentIndex) && isInYBounds(currentClosest.rect.position, node.rect.position, nod.rect.position))
                {
                    currentClosest = nod;
                    connectionIsMade = true;
                }
            }
        }

        if (node.children.Count > 0)
            node.children.Clear();

        if (!connectionIsMade)
            return;

        if (currentClosest.currentIndex > node.currentIndex && isInXBounds(currentClosest.rect.center, node.rect.center))
        {
            if (!IsDescendantOf(currentClosest, node))
            {
                node.children.Add(currentClosest);
            }
        }
    }

    public void CheckandConnectConditionalNode(ConversationNode node)
    {
        if (!(node.nodeType == ConversationNodeType.Condition))
            return;

        if (nodes.Count < 2)
        {
            node.children.Clear();
            return;
        }

        node.children.Clear();

        for (int i = 0; i < node.optionsPositions.Count; i++)
        {
            ConversationNode currentClosest = CheckAndConnectNode(node.optionsPositions[i], node);

            if (currentClosest != null)
            {
                if (currentClosest.currentIndex > node.currentIndex && isInXBounds(currentClosest.rect.position, node.optionsPositions[i].position))
                {
                    if(!IsDescendantOf(currentClosest, node))
                    {
                        node.children.Add(currentClosest);
                    }
                }
            }
        }
    }

    ConversationNode CheckAndConnectNode(Rect optionPosition, ConversationNode node)
    {
        bool connectionIsMade = false;
        ConversationNode currentClosest = null;
        foreach (ConversationNode nod in nodes)
        {
            if (nod != node && isInXBounds(nod.rect.position, optionPosition.position))
            {
                if (currentClosest == null)
                {
                    currentClosest = nod;
                    connectionIsMade = true;
                }
                else if (isInIndexBounds(node.currentIndex, nod.currentIndex) && isInYBounds(currentClosest.rect.position, optionPosition.position, nod.rect.position))
                {
                    currentClosest = nod;
                    connectionIsMade = true;
                }
            }
        }

        if (connectionIsMade)
            return currentClosest;
        else
            return null;
    }

    void HandleZooming()
    {
        Event e = Event.current;

        if (e.type == EventType.ScrollWheel && (e.control || e.command))
        {
            float scrollDelta = e.delta.y;
            Debug.Log("Ctrl + Scroll detected, delta: " + scrollDelta);

            if(windowScale + scrollDelta * 0.001f > 0)
            {
                windowScale += scrollDelta * 0.001f;
            }
            float temp = windowScale + scrollDelta;
            Debug.Log("Window Scale + Delta: " + temp);

            e.Use();
        }
    }

    bool isInIndexBounds(int index1, int index2)
    {  
        if(index1 < index2)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    bool isInYBounds(Vector2 pos1, Vector2 pos2, Vector2 pos3)
    {
        if (Mathf.Abs(pos1.y - pos2.y) >= Mathf.Abs(pos3.y - pos2.y))
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    bool isInXBounds(Vector2 pos1, Vector2 pos2)
    {
        if (Mathf.Abs(pos2.x - pos1.x) < 10f)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    private void ProcessEvents(Event e)
    {
        if (e.type == EventType.MouseDrag && e.button == 2)
        {
            drag = e.delta;
            for (int i = 0; i < nodes.Count; i++)
                nodes[i].rect.position += drag;

            GUI.changed = true;
        }
    }

    private Rect CalculateCanvas(List<ConversationNode> nodes)
    {
        if (nodes == null || nodes.Count == 0)
            return new Rect(0, 0, 2000, 2000);

        float minX = nodes.Min(n => n.rect.x);
        float minY = nodes.Min(n => n.rect.y);
        float maxX = nodes.Max(n => n.rect.xMax);
        float maxY = nodes.Max(n => n.rect.yMax);

        return new Rect(0, 0, Mathf.Max(2000, maxX - minX + 200), Mathf.Max(2000, maxY - minY + 200));
    }

    private bool IsDescendantOf(ConversationNode node, ConversationNode potentialParent)
    {
        if (node == potentialParent) return true;
        foreach (var c in node.children)
            if (IsDescendantOf(c, potentialParent)) return true;
        return false;
    }

    int GetNextNodeId()
    {
        return nodes.Count == 0 ? 0 : nodes.Max(n => n.id) + 1;
    }

    void SyncChildIds()
    {
        foreach (var node in nodes)
        {
            node.childIds.Clear();
            foreach (var child in node.children)
            {
                node.childIds.Add(child.id);
            }
        }
    }

    void RebuildChildrenFromIds()
    {
        var lookup = nodes.ToDictionary(n => n.id, n => n);

        foreach (var node in nodes)
        {
            node.children.Clear();
            foreach (int id in node.childIds)
            {
                if (lookup.TryGetValue(id, out var child))
                    node.children.Add(child);
            }
        }
    }
}
