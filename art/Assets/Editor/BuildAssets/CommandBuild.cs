using UnityEditor;
using System.IO;
using UnityEngine;

public class CommandBuild
{
    public static void DelNonVersion()
    {
        string strReadFilePath = Application.dataPath + "/SvnStatus.txt";

        if (File.Exists(strReadFilePath) == false)
            return;

        using (StreamReader srReadFile = new StreamReader(strReadFilePath))
        {
            var assetsFlag = "Assets/";
            var streamingAssetsFlag = "StreamingAssets/";
            while (!srReadFile.EndOfStream)
            {
                string strReadLine = srReadFile.ReadLine();
                if (strReadLine.IndexOf("?") == 0)
                {
                    strReadLine = strReadLine.Replace(@"\", @"/");
                    string filePath = strReadLine.Replace("?       ", "").Trim();
                    int indexAssets = filePath.IndexOf(assetsFlag);
                    int indexStreamingAssets = filePath.IndexOf(streamingAssetsFlag);

                    if (indexAssets != -1 && (indexStreamingAssets == -1 || indexAssets < indexStreamingAssets))
                    {
                        filePath = filePath.Substring(indexAssets + assetsFlag.Length);
                    }

                    FileUtil.DeleteFileOrDirectory(assetsFlag + filePath);
                }
            }
        };

        FileUtil.DeleteFileOrDirectory("Assets/SvnStatus.txt");

        AssetDatabase.Refresh();
    }

    public static void BuildAssets()
    {
        BuildAssetBundle.BuildAssetsWithBuildTarget(BuildAssetBundleOptions.ForceRebuildAssetBundle, GetBuildTarget());
    }

    public static void BuildGame()
    {
        string[] levels = GetPublishType() == "Development" ? GetDevelopmentBuildLevels() : GetProductBuildLevels();
        switch (GetBuildTarget())
        {
            case BuildTarget.Android:
                BuildGameAndroid(levels);
                break;
            case BuildTarget.iOS:
                BuildGameIOS(levels);
                break;
        }
    }

    static void BuildGameAndroid(string[] levels)
    {
        BuildPipeline.BuildPlayer(levels, "Assets/game.apk", BuildTarget.Android, BuildOptions.None);
    }

    static void BuildGameIOS(string[] levels)
    {

    }

    private static string[] GetProductBuildLevels()
    {
        string[] levels = { "Assets/Scenes/Login.unity", "Assets/Scenes/Scene.unity", "Assets/Scenes/Combat.unity" };
        return levels;
    }

    private static string[] GetDevelopmentBuildLevels()
    {
        string[] levels = { "Assets/Scenes/Login.unity", "Assets/Scenes/Scene.unity", "Assets/Scenes/Combat.unity" };
        return levels;
    }

    public static BuildTarget GetBuildTarget()
    {
        string runtimePlatform = GetExternalArg("RuntimePlatform");

        switch (runtimePlatform)
        {
            case "ANDROID":
                return BuildTarget.Android;
            case "IOS":
                return BuildTarget.iOS;
        }

        return BuildTarget.NoTarget;
    }

    public static string GetPublishType()
    {
        return GetExternalArg("PublishType");
    }

    public static string GetExternalArg(string name)
    {
        foreach (string arg in System.Environment.GetCommandLineArgs())
        {
            if (arg.StartsWith(name))
            {
                return arg.Split('-')[1];
            }
        }

        return "";
    }
}