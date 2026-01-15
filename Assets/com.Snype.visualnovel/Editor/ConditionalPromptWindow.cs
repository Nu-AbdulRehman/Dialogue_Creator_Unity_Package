using UnityEditor;
using UnityEngine;

public class ConditionalPromptWindow : EditorWindow
{
    private Conversation convo;
    private System.Action<Conversation> onDone;
    int selectedIndex;

    public static void Show(Conversation convo, System.Action<Conversation> onDone)
    {
        ConditionalPromptWindow window = ScriptableObject.CreateInstance<ConditionalPromptWindow>();
        window.convo = convo;
        window.onDone = onDone;
        window.titleContent = new GUIContent("Conditional Node");
        window.ShowPopup();
        window.position = new Rect(Screen.width / 2f, Screen.height / 2f, 300, 100);
    }

    private void OnGUI()
    {
        ShowPrompt();
    }

    void ShowPrompt()
    {
        string[] options = { "2", "3", "4", "5" };

        GUILayout.Label("How many options in the conditional node?", EditorStyles.wordWrappedLabel);
        GUILayout.Space(10);
        selectedIndex = EditorGUILayout.Popup("Options", selectedIndex, options);
        if (GUILayout.Button("Done"))
        {
            PlayerPrefs.SetInt("ConditionsOption", int.Parse(options[selectedIndex]));
            onDone?.Invoke(convo);
            Close();
        }
    }

    void OnLostFocus()
    {
        Close();
    }
}
