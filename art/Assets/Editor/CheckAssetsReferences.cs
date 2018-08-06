using UnityEditor;
using System.IO;
using System.Collections.Generic;

namespace Assets.Editor
{
    public class CheckAssetsReferences : EditorWindow
    {
        private static string[] sCheckPath = new string[] { "Assets/RawResourcesExport/GameScenes" };
        private static int count = 0;

        [MenuItem("Assets/Check/CheckReferences", false, 0)]
        static public void CheckReferences()
        {
            string[] assetGUIDs = Selection.assetGUIDs;

            if (assetGUIDs.Length > 0)
            {
                string path = AssetDatabase.GUIDToAssetPath(assetGUIDs[0]);
                string fileName = Path.GetFileName(path);

                UnityEngine.Debug.LogError("开始检测:" + path);

                count = 0;

                foreach (string str in sCheckPath)
                {
                    Check(path, new DirectoryInfo(str));
                }

                UnityEngine.Debug.LogError(string.Format("检测{0}引用完毕,共有{1}处引用", path, count));
            }
        }

        //直接写count++会报错，后改用调用方法的方式
        static void AddCount()
        {
            count++;
        }

        static void Check(string assetPath, DirectoryInfo directoryInfo)
        {
            FileInfo[] fileInfoList = GetFileInfoList(directoryInfo);

            for (int i = 0, count = fileInfoList.Length; i < count; i++)
            {
                FileInfo fileInfo = fileInfoList[i];

                string itemPath = fileInfo.FullName.Remove(0, fileInfo.FullName.IndexOf("Assets"));
                string[] dependPathList = AssetDatabase.GetDependencies(new string[] { itemPath });
                foreach (string path in dependPathList)
                {
                    if (path == assetPath)
                    {
                        AddCount();
                        UnityEngine.Debug.LogError(string.Format("以下文件有引用{0}：{1}", Path.GetFileName(assetPath), fileInfo.FullName));
                        break;
                    }
                }
            }

            DirectoryInfo[] folderInfoList = GetFolderInfoList(directoryInfo);
            for (int j = 0, count = folderInfoList.Length; j < count; j++)
            {
                Check(assetPath, folderInfoList[j]);
            }
        }

        public static FileInfo[] GetFileInfoList(DirectoryInfo directoryInfo, string postfix = "")
        {
            FileInfo[] rawFileList = directoryInfo.GetFiles();
            List<FileInfo> fileList = new List<FileInfo>();

            foreach (FileInfo info in rawFileList)
            {
                if (string.IsNullOrEmpty(postfix))
                {
                    if (info.Name.EndsWith("meta"))
                        continue;

                    if (info.Name.EndsWith("mat"))
                        continue;

                    if (info.Name.EndsWith("exr"))
                        continue;

                    if (info.Name.EndsWith("asset"))
                        continue;

                    if (info.Name.EndsWith("txt"))
                        continue;
                }
                else
                {
                    if (info.Name.EndsWith(postfix) == false)
                        continue;
                }

                fileList.Add(info);
            }

            return fileList.ToArray();
        }

        public static DirectoryInfo[] GetFolderInfoList(DirectoryInfo directoryInfo)
        {
            DirectoryInfo[] rawFolderList = directoryInfo.GetDirectories();
            List<DirectoryInfo> folderList = new List<DirectoryInfo>();

            foreach (DirectoryInfo info in rawFolderList)
            {
                if (info.Name.EndsWith(".svn"))
                    continue;

                folderList.Add(info);
            }

            return folderList.ToArray();
        }
    }
}
