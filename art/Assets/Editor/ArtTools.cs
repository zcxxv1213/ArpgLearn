using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
public class ArtTools 
{
    [MenuItem("Assets/检查时装骨骼")]
    public static void CheckBones()
    {
        var rootBone = Selection.activeGameObject.GetComponentInChildren<Animator>().transform.Find("Bip001");
        var bones = rootBone.GetComponentsInChildren<Transform>(true);
        Dictionary<string, Transform> bonesMap = new Dictionary<string, Transform>();
        foreach (var bone in bones)
        {
            bonesMap[bone.name] = bone;
        }
        string directory = "Assets/RawResources/role/fashion";
        string fileName = Path.GetFileName(AssetDatabase.GetAssetPath(Selection.activeGameObject));
        string compareStr = fileName.Contains("nanxing") ? "nanxing":"nvxing";
        string[] files = Directory.GetFiles(directory, "*.prefab");
        foreach (var file in files)
        {
            if (file.Contains(compareStr))
            {
                var go = AssetDatabase.LoadAssetAtPath<GameObject>(file);
                if (go)
                {
                    CheckGameObject(go, bonesMap, fileName, Path.GetFileName(file));
                }
            }
        }
        Debug.LogError("检查完成");
    }

    public static void CheckGameObject(GameObject go, Dictionary<string, Transform> bonesMap,string file1,string file2)
    {
        var smrs = go.GetComponentsInChildren<SkinnedMeshRenderer>(true);
        foreach (var smr in smrs)
        {
            foreach (var bone in smr.bones)
            {
                if (!bonesMap.ContainsKey(bone.name))
                {
                    Debug.LogError(string.Format("{0}中的骨骼{1}在{2}中查找不到对应的骨骼", file2, bone.name, file1));
                }

            }

        }
    }
}
