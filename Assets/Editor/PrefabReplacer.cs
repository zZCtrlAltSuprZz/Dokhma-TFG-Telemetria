using UnityEngine;
using UnityEditor;

public class PrefabReplacer : EditorWindow
{
    private GameObject prefab;

    [MenuItem("Tools/Prefab Replacer")]
    static void Open()
    {
        GetWindow<PrefabReplacer>("Prefab Replacer");
    }

    void OnGUI()
    {
        prefab = (GameObject)EditorGUILayout.ObjectField(
            "Prefab", prefab, typeof(GameObject), false);

        if (GUILayout.Button("Replace Selected"))
        {
            foreach (GameObject obj in Selection.gameObjects)
            {
                GameObject newObj = (GameObject)PrefabUtility
                    .InstantiatePrefab(prefab, obj.transform.parent);
                newObj.transform.position = obj.transform.position;
                newObj.transform.rotation = obj.transform.rotation;
                newObj.transform.localScale = obj.transform.localScale;
                Undo.RegisterCreatedObjectUndo(newObj, "Replace");
                Undo.DestroyObjectImmediate(obj);
            }
        }
    }
}