using System.Collections.Generic;
using UnityEngine;

public class RuntimeFunctionRegistry : MonoBehaviour
{
    [Header("Scene scripts that can be called by RunFunction nodes")]
    public MonoBehaviour[] availableScripts;

    private Dictionary<string, MonoBehaviour> lookup = new Dictionary<string, MonoBehaviour>();

    private void Awake()
    {
        lookup.Clear();
        foreach (var script in availableScripts)
        {
            if (script != null)
                lookup[script.GetType().Name] = script;
        }
    }

    public MonoBehaviour GetScript(string typeName)
    {
        lookup.TryGetValue(typeName, out MonoBehaviour mb);
        return mb;
    }
}
