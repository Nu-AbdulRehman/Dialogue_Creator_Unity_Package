using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public class EditorQuitHandler: MonoBehaviour
{
    static EditorQuitHandler()
    {
        EditorApplication.wantsToQuit += OnWantsToQuit;
    }

    private static bool OnWantsToQuit()
    {
        Debug.Log("Saving Conversations Database before quitting...");
        AssetDatabase.SaveAssets();
        return true;
    }
}
