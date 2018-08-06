using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Reflection;

[CustomEditor(typeof(Transform))]
public class TransformInspector : Editor
{
    static bool bFastPut = false;
    static string strChildName = "";
    public override void OnInspectorGUI()
    {
        Transform trans = target as Transform;
        EditorGUIUtility.LookLikeControls(15f);
        Vector3 pos;
        Vector3 rot;
        Vector3 scale;

        // Position
        EditorGUILayout.BeginHorizontal();
        {
            if (DrawButton("P", "Reset Position", IsResetPositionValid(trans), 20f))
            {
                Undo.RegisterUndo(trans, "Reset Position");
                trans.localPosition = Vector3.zero;
            }
            pos = DrawVector3(trans.localPosition);
        }
        EditorGUILayout.EndHorizontal();

        // Rotation
        EditorGUILayout.BeginHorizontal();
        {
            if (DrawButton("R", "Reset Rotation", IsResetRotationValid(trans), 20f))
            {
                Undo.RegisterUndo(trans, "Reset Rotation");
                trans.localEulerAngles = Vector3.zero;
            }
            rot = DrawVector3(trans.localEulerAngles);
        }
        EditorGUILayout.EndHorizontal();

        // Scale
        EditorGUILayout.BeginHorizontal();
        {
            if (DrawButton("S", "Reset Scale", IsResetScaleValid(trans), 20f))
            {
                Undo.RegisterUndo(trans, "Reset Scale");
                trans.localScale = new Vector3(1, 1, 1);
            }
            scale = DrawVector3(trans.localScale);
        }
        EditorGUILayout.EndHorizontal();

        // 如果有数值更改，设置 transform 数值
        if (GUI.changed)
        {
            Undo.RegisterUndo(trans, "Transform Change");
            trans.localPosition = Validate(pos);
            trans.localEulerAngles = Validate(rot);
            trans.localScale = Validate(scale);
        }

        // Copy
        EditorGUILayout.BeginHorizontal();
        {
            bFastPut = EditorGUILayout.Toggle(new GUIContent("", "Mouse Put"), bFastPut, EditorStyles.miniButton, GUILayout.Width(20));
            Color c = GUI.color;
            GUI.color = new Color(1f, 1f, 0.5f, 1f);
            //GUILayout.Button(, GUILayout.Width(width));
            if (GUILayout.Button("Copy Local", EditorStyles.miniButtonLeft))
            {
                v3Pos = trans.localPosition;
                qRotate = trans.localRotation;
                v3Scale = trans.localScale;
            }

            GUI.color = new Color(1f, 0.5f, 0.5f, 1f);
            if (GUILayout.Button("Paste Local", EditorStyles.miniButtonRight))
            {
                Undo.RegisterUndo(trans, "Paste Local");
                trans.localPosition = v3Pos;
                trans.localRotation = qRotate;
                trans.localScale = v3Scale;
            }

            GUI.color = c;
        }

        EditorGUILayout.EndHorizontal();

        // Copy
        EditorGUILayout.BeginHorizontal();
        {
            if (GUILayout.Button("Paste Pos", EditorStyles.miniButtonLeft))
            {
                Undo.RegisterUndo(trans, "Paste Pos");
                trans.localPosition = v3Pos;
            }

            if (GUILayout.Button("Paste Rot", EditorStyles.miniButtonMid))
            {
                Undo.RegisterUndo(trans, "Paste Rot");
                trans.localRotation = qRotate;
            }

            if (GUILayout.Button("Paste Sca", EditorStyles.miniButtonRight))
            {
                Undo.RegisterUndo(trans, "Paste Sca");
                trans.localScale = v3Scale;
            }
        }

        EditorGUILayout.EndHorizontal();

        //paste ui
        EditorGUILayout.BeginHorizontal();
        {
            if (GUILayout.Button("Paste UIPosX", EditorStyles.miniButtonLeft))
            {
                System.Type T = typeof(GUIUtility);
                PropertyInfo systemCopyBufferProperty = T.GetProperty("systemCopyBuffer", BindingFlags.Static | BindingFlags.NonPublic);
                string value = (string)systemCopyBufferProperty.GetValue(T, null);

                Vector3 localPos = trans.localPosition;

                trans.localPosition = new Vector3(float.Parse(value) - 960 / 2, localPos.y, localPos.z);
            }

            if (GUILayout.Button("Paste UIPosY", EditorStyles.miniButtonMid))
            {
                System.Type T = typeof(GUIUtility);
                PropertyInfo systemCopyBufferProperty = T.GetProperty("systemCopyBuffer", BindingFlags.Static | BindingFlags.NonPublic);
                string value = (string)systemCopyBufferProperty.GetValue(T, null);

                Vector3 localPos = trans.localPosition;

                trans.localPosition = new Vector3(localPos.x, 640 / 2 - float.Parse(value), localPos.z);
            }
        }

        EditorGUILayout.EndHorizontal();

        // AddChild
        EditorGUILayout.BeginHorizontal();
        {
            if (GUILayout.Button("Add Child", EditorStyles.miniButton, GUILayout.Width(80)))
            {
                SelectionTools.CreateChild(strChildName);
            }

            EditorGUILayout.LabelField("Name", GUILayout.Width(40));
            strChildName = EditorGUILayout.TextField(strChildName);

        }
        EditorGUILayout.EndHorizontal();
    }

    void OnSceneGUI()
    {
        if (!bFastPut)
        {
            return;
        }

        if (Event.current.type == EventType.MouseDown && Event.current.button == 0 && (Event.current.control || Event.current.alt))
        {
            //create a ray to get where we clicked on terrain/ground/objects in scene view and pass in mouse position
            Transform trans = target as Transform;
            Ray worldRay = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
            RaycastHit hitInfo;

            //ray hit something
            if (Physics.Raycast(worldRay, out hitInfo))
            {
                //call this method when you've used an event.
                //the event's type will be set to EventType.Used,
                //causing other GUI elements to ignore it
                Event.current.Use();

                //place a waypoint at clicked point
                Undo.RegisterUndo(trans, "Hit");
                trans.position = hitInfo.point;
            }
        }
    }

    static Vector3 v3Pos = Vector3.zero;
    static Quaternion qRotate = Quaternion.identity;
    static Vector3 v3Scale = Vector3.one;
    static bool DrawButton(string title, string tooltip, bool enabled, float width)
    {
        if (enabled)
        {
            Color color = GUI.color;
            GUI.color = new Color(1f, 0.5f, 0.5f, 1f);
            bool hexart = GUILayout.Button(new GUIContent(title, tooltip), EditorStyles.miniButton, GUILayout.Width(width));
            GUI.color = color;
            return hexart;
        }
        else
        {
            Color color = GUI.color;
            GUI.color = new Color(1f, 0.5f, 0.5f, 0.25f);
            GUILayout.Button(new GUIContent(title, tooltip), EditorStyles.miniButton, GUILayout.Width(width));
            GUI.color = color;
            return false;
        }
    }

    static Vector3 DrawVector3(Vector3 value)
    {
        GUILayoutOption opt = GUILayout.MinWidth(30f);
        Color color = GUI.color;
        GUI.color = new Color(1f, 0.5f, 0.5f, 1f);
        value.x = EditorGUILayout.FloatField("X", value.x, opt);
        GUI.color = new Color(0.5f, 1f, 0.5f, 1f);
        value.y = EditorGUILayout.FloatField("Y", value.y, opt);
        GUI.color = new Color(0.5f, 0.75f, 1f, 1f);
        value.z = EditorGUILayout.FloatField("Z", value.z, opt);
        GUI.color = color;
        return value;
    }

    static bool IsResetPositionValid(Transform targetTransform)
    {
        Vector3 v = targetTransform.localPosition;
        return (v.x != 0f || v.y != 0f || v.z != 0f);
    }

    static bool IsResetRotationValid(Transform targetTransform)
    {
        Vector3 v = targetTransform.localEulerAngles;
        return (v.x != 0f || v.y != 0f || v.z != 0f);
    }

    static bool IsResetScaleValid(Transform targetTransform)
    {
        Vector3 v = targetTransform.localScale;
        return (v.x != 1f || v.y != 1f || v.z != 1f);
    }

    static Vector3 Validate(Vector3 vector)
    {
        vector.x = float.IsNaN(vector.x) ? 0f : vector.x;
        vector.y = float.IsNaN(vector.y) ? 0f : vector.y;
        vector.z = float.IsNaN(vector.z) ? 0f : vector.z;
        return vector;
    }
}

public class SelectionTools
{
    //[MenuItem("Hexart/Toggle 'Active' #&a")]
    //static void ActivateDeactivate()
    //{
    //    if (HasValidTransform())
    //    {
    //        GameObject[] gos = Selection.gameObjects;
    //        bool val = !Selection.activeGameObject.activeSelf;
    //        foreach (GameObject go in gos) go.SetActive(val);
    //    }
    //}

    //[MenuItem("CONTEXT/Transform/Create Child")]
    //static void CreateLocalGameObject2()
    //{
    //    CreateChild("GameObject");
    //}

    public static void CreateChild(string name)
    {
        // 无法撤销
        Undo.RegisterSceneUndo("Add New Child");
        GameObject newGameObject = new GameObject();
        if (name == "")
        {
            name = "GameObject";
        }

        newGameObject.name = name;

        if (Selection.activeTransform != null)
        {
            newGameObject.transform.parent = Selection.activeTransform;
            //newGameObject.name = "Child";
            newGameObject.transform.localPosition = Vector3.zero;
            newGameObject.transform.localRotation = Quaternion.identity;
            newGameObject.transform.localScale = new Vector3(1f, 1f, 1f);
            newGameObject.layer = Selection.activeGameObject.layer;
        }

        Selection.activeTransform = newGameObject.transform;
    }

    //[MenuItem("Hexart/List Dependencies #&i")]
    //static void ListDependencies()
    //{
    //    if (HasValidSelection())
    //    {
    //        Debug.Log("Asset dependencies:\n" + GetDependencyText(Selection.objects));
    //    }
    //}


    #region Helper Functions

    class AssetEntry
    {
        public string path;
        public List<System.Type> types = new List<System.Type>();
    }

    static bool HasValidSelection()
    {
        if (Selection.objects == null || Selection.objects.Length == 0)
        {
            Debug.LogWarning("You must select an object first");
            return false;
        }
        return true;
    }

    static bool HasValidTransform()
    {
        if (Selection.activeTransform == null)
        {
            Debug.LogWarning("You must select an object first");
            return false;
        }
        return true;
    }

    static bool PrefabCheck()
    {
        if (Selection.activeTransform != null)
        {
            // 如果是 prefab 实例则警告
            PrefabType type = PrefabUtility.GetPrefabType(Selection.activeGameObject);

            if (type == PrefabType.PrefabInstance)
            {
                return EditorUtility.DisplayDialog("Losing prefab",
                    "This action will lose the prefab connection. Are you sure you wish to continue?",
                    "Continue", "Cancel");
            }
            return true;
        }
        return false;
    }

    static List<AssetEntry> GetDependencyList(Object[] objects)
    {
        Object[] deps = EditorUtility.CollectDependencies(objects);

        List<AssetEntry> list = new List<AssetEntry>();

        foreach (Object obj in deps)
        {
            string path = AssetDatabase.GetAssetPath(obj);

            if (!string.IsNullOrEmpty(path))
            {
                bool found = false;
                System.Type type = obj.GetType();

                foreach (AssetEntry ent in list)
                {
                    if (string.Equals(ent.path, path))
                    {
                        if (!ent.types.Contains(type)) ent.types.Add(type);
                        found = true;
                        break;
                    }
                }

                if (!found)
                {
                    AssetEntry ent = new AssetEntry();
                    ent.path = path;
                    ent.types.Add(type);
                    list.Add(ent);
                }
            }
        }

        deps = null;
        objects = null;
        return list;
    }

    static string RemovePrefix(string text)
    {
        text = text.Replace("UnityEngine.", "");
        text = text.Replace("UnityEditor.", "");
        return text;
    }

    static string GetDependencyText(Object[] objects)
    {
        List<AssetEntry> dependencies = GetDependencyList(objects);
        List<string> list = new List<string>();
        string text = "";

        foreach (AssetEntry ae in dependencies)
        {
            text = ae.path.Replace("Assets/", "");

            if (ae.types.Count > 1)
            {
                text += " (" + RemovePrefix(ae.types[0].ToString());

                for (int i = 1; i < ae.types.Count; ++i)
                {
                    text += ", " + RemovePrefix(ae.types[i].ToString());
                }

                text += ")";
            }
            list.Add(text);
        }

        list.Sort();

        text = "";
        foreach (string s in list) text += s + "\n";
        list.Clear();
        list = null;

        dependencies.Clear();
        dependencies = null;
        return text;
    }
    #endregion
}
