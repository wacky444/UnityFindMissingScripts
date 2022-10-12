using UnityEditor;
using UnityEngine;

// To use this script, press EditorScripts in the top bar of Unity
public class FindMissingScriptsRecursivelyAndRemove : EditorWindow
{
    private static int _goCount;
    private static int _componentsCount;
    private static int _missingCount;

    private static bool _bHaveRun;

    public void OnGUI()
    {
        if (GUILayout.Button("Find Missing Scripts in selected GameObjects"))
        {
            FindInSelected();
        }

        if (!_bHaveRun) return;

        EditorGUILayout.TextField($"{_goCount} GameObjects Selected");
        if (_goCount > 0) EditorGUILayout.TextField($"{_componentsCount} Components");
        if (_goCount > 0) EditorGUILayout.TextField($"{_missingCount} Deleted");
    }

    [MenuItem("EditorScripts/FindMissingScriptsRecursivelyAndRemove")]
    public static void ShowWindow()
    {
        GetWindow(typeof(FindMissingScriptsRecursivelyAndRemove));
    }

    private static void FindInSelected()
    {
        GameObject[] gameObject = Selection.gameObjects;
        _goCount = 0;
        _componentsCount = 0;
        _missingCount = 0;
        foreach (GameObject g in gameObject)
        {
            FindInGo(g);
        }

        _bHaveRun = true;
        Debug.Log($"Searched {_goCount} GameObjects, {_componentsCount} components, found {_missingCount} missing");

        AssetDatabase.SaveAssets();
    }

    private static void FindInGo(GameObject g)
    {
        _goCount++;
        Component[] components = g.GetComponents<Component>();

        for (int i = 0; i < components.Length; i++)
        {
            _componentsCount++;
            if (components[i] != null) continue;
            _missingCount++;
            string s = g.name;
            Transform t = g.transform;
            while (t.parent != null)
            {
                s = t.parent.name + "/" + s;
                t = t.parent;
            }

            Debug.Log($"{s} has a missing script at {i}", g);

            SerializedObject serializedObject = new SerializedObject(g);

            SerializedProperty prop = serializedObject.FindProperty("m_Component");
            GameObjectUtility.RemoveMonoBehavioursWithMissingScript(g);
        }

        foreach (Transform childT in g.transform)
        {
            FindInGo(childT.gameObject);
        }
    }
}