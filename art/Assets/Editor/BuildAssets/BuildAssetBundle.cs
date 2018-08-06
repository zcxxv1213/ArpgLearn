using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
class Depend
{
    public string path;
    public string owner;
}
public class BuildAssetBundle : Editor
{
    static void RemoveAssetBundleNames()
    {
        string[] allAssetBundleNames = AssetDatabase.GetAllAssetBundleNames();
        for (int i = 0; i < allAssetBundleNames.Length; i++)
        {
            AssetDatabase.RemoveAssetBundleName(allAssetBundleNames[i], true);
        }
    }
    static void BuildAssets(BuildAssetBundleOptions options, BuildTarget target)
    {
        RemoveAssetBundleNames();
        string path = "Assets/StreamingAssets";
        string rawResourcesPath = "Assets/RawResources";
        string[] allFiles = System.IO.Directory.GetFiles(rawResourcesPath, "*", System.IO.SearchOption.AllDirectories);

        List<string> rawResources = new List<string>();

        Dictionary<string, Depend> references = new Dictionary<string, Depend>();

        for (int i = 0; i < allFiles.Length; i++)
        {
            AssetImporter assetImporter = AssetImporter.GetAtPath(allFiles[i]);
            if (assetImporter)
            {
                string rawResource = allFiles[i].Replace(@"\", "/");
                string extension = System.IO.Path.GetExtension(rawResource);
                rawResources.Add(rawResource);
                if (IsSceneDepend(rawResource)) continue;
                assetImporter.assetBundleName = rawResource.Replace(extension, "").Replace(rawResourcesPath + "/", "");
                //assetImporter.assetBundleVariant = extension.Replace(".", "");
            }
        }

        for (int i = 0; i < rawResources.Count; i++)
        {
            GetDependencies(rawResources[i], references, rawResources);
        }

        foreach (Depend depend in references.Values)
        {
            AssetImporter assetImporter = AssetImporter.GetAtPath(depend.path);
            if (assetImporter)
            {

                if (depend.path.EndsWith(".cs")) continue;
                if (IsSceneDepend(depend.path)) continue;
                if (rawResources.Contains(depend.path))
                {
                    continue;
                }
                if (depend.owner == null) continue;

                string extension = System.IO.Path.GetExtension(depend.owner);
                assetImporter.assetBundleName = depend.owner.Replace(extension, "").Replace(rawResourcesPath + "/", "");
                //assetImporter.assetBundleVariant = extension.Replace(".","");
            }
        }

        if (System.IO.Directory.Exists(path) == false)
        {
            System.IO.Directory.CreateDirectory(path);
        }


        AssetBundleManifest manifest = BuildPipeline.BuildAssetBundles(path, options | BuildAssetBundleOptions.DisableWriteTypeTree, target);
        Clear(manifest);
        RemoveAssetBundleNames();
        AssetDatabase.Refresh();
    }

    static void Clear(AssetBundleManifest manifest)
    {
        string[] temp = manifest.GetAllAssetBundles();
        List<string> allBundles = new List<string>();
        for (int i = 0; i < temp.Length; i++)
        {
            string file =  "Assets/StreamingAssets/"+temp[i];
            allBundles.Add(file);
            allBundles.Add(file + ".meta");
            allBundles.Add(file + ".manifest");
            allBundles.Add(file + ".manifest.meta");
        }
        allBundles.Add("Assets/StreamingAssets/StreamingAssets");
        allBundles.Add("Assets/StreamingAssets/StreamingAssets.meta");
        allBundles.Add("Assets/StreamingAssets/StreamingAssets.manifest.meta");
        allBundles.Add("Assets/StreamingAssets/StreamingAssets.manifest");

        string path = "Assets/StreamingAssets";
        string[] allFiles = System.IO.Directory.GetFiles(path, "*", System.IO.SearchOption.AllDirectories);
        for (int i = 0; i < allFiles.Length; i++)
        {
            string file = allFiles[i].Replace(@"\", "/");
            if (!allBundles.Contains(file))
            {
                if (!System.IO.Directory.Exists(file.Replace(".meta", "")))
                {
                    Debug.Log("删除文件->"+file);
                    FileUtil.DeleteFileOrDirectory(file);
                }
            }
        }
    }
     //[MenuItem("Tools/CheckBundles")]
    static void CheckBundles()
    {
        string path = "Assets/StreamingAssets";
        string[] allFiles = System.IO.Directory.GetFiles(path, "*", System.IO.SearchOption.AllDirectories);
        for (int i = 0; i < allFiles.Length; i++)
        {
            Debug.LogWarning(allFiles[i].Replace(@"\", "/"));
        }
    }
    static bool IsSceneDepend(string file)
    {
        string directory = System.IO.Path.GetDirectoryName(file);
        return System.IO.File.Exists(directory+".unity");
    }
    static void GetDependencies(string file, Dictionary<string, Depend> references, List<string> rawResources)
    {
        string[] dependencies = AssetDatabase.GetDependencies(file);
        for (int i = 0; i < dependencies.Length; i++)
        {
            string dependPath = dependencies[i];
            if (references.ContainsKey(dependPath))
            {
                //公用资源
                references[dependPath].owner = "common/" + System.IO.Path.GetFileName(references[dependPath].path);
            }
            else
            {
                Depend depend = new Depend();
                depend.owner = null;
                depend.path = dependPath;
                references.Add(dependPath, depend);
            }

            if (rawResources.Contains(dependPath))
            {
                references[dependPath].owner = dependPath;
            }
        }
    }

    [MenuItem("Tools/BuildAssets")]
    static void ToolsBuildAssets()
    {
        BuildAssetsWithBuildTarget(BuildAssetBundleOptions.None, EditorUserBuildSettings.activeBuildTarget);
    }

    public static void BuildAssetsWithBuildTarget(BuildAssetBundleOptions options, BuildTarget target)
    {
        BuildAssets(options, target);
    }
}
