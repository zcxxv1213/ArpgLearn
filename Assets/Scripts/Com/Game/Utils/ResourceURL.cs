using System.IO;
using UnityEngine;

namespace Assets.Scripts.Com.Game.Utils
{
    public static class ResourceURL
    {
        public static string RAW_STREAMING_ASSETS { get; private set; }

        public static string PERSISTENT_STREAMING_ASSETS { get; private set; }
        private static string mPersistentDataPath { get; set; }
        private static string mPersistentDataExternalPath { get; set; }
        private static string mGamePersistentDataRawPath { get; set; }
        private static string mGamePersistentDataExternalPath { get; set; }
        private static string mGameStreamingAssetsRawPath { get; set; }
        private static string mGameStreamingAssetsExternalPath { get; set; }

        public const string FILE_FLAG = "file://";

        private const string SCENE_PATH = "scenes/";

        private const string EXTERNAL_PATH = "external/";

        private const string MODEL_PATH = "model/";

        private const string UI_FX_PATH = "ui_fx/";

        const string cStreamingAssetsPath = @"{0}/StreamingAssets/{1}";
        static ResourceURL()
        {
            if (Application.platform == RuntimePlatform.Android)
            {
                RAW_STREAMING_ASSETS = Application.streamingAssetsPath;
                PERSISTENT_STREAMING_ASSETS = FILE_FLAG + Application.persistentDataPath;
            }
            else
            {
                RAW_STREAMING_ASSETS = FILE_FLAG + Application.streamingAssetsPath;
                PERSISTENT_STREAMING_ASSETS = FILE_FLAG + Application.persistentDataPath;
            }
            mPersistentDataPath = Application.persistentDataPath;
            mPersistentDataExternalPath = mPersistentDataPath + "/StreamingAssets/external/";
            mGamePersistentDataRawPath = PERSISTENT_STREAMING_ASSETS + "/StreamingAssets/";
            mGamePersistentDataExternalPath = PERSISTENT_STREAMING_ASSETS + "/StreamingAssets/external/";
            mGameStreamingAssetsRawPath = RAW_STREAMING_ASSETS + "/";
            mGameStreamingAssetsExternalPath = RAW_STREAMING_ASSETS + "/external/";

        }

        public static string GetUrl(string path)
        {
            if (System.IO.File.Exists(mPersistentDataExternalPath + path))
            {
                return mGamePersistentDataRawPath + path;
            }
            return mGameStreamingAssetsRawPath + path;
        }

        public static string GetExternalUrl(string path)
        {
            if (System.IO.File.Exists(mPersistentDataExternalPath + path))
            {
                return mGamePersistentDataExternalPath + path;
            }
            return mGameStreamingAssetsExternalPath + path;
        }

        public static string GetRawFilePath(string path)
        {
            return Application.streamingAssetsPath + "/" + path;
        }

        public static string GetScenePath()
        {
            return SCENE_PATH;
        }

        public static string GetExternalPath(string assetName)
        {
            return EXTERNAL_PATH + assetName;
        }
        public static string GetModelPath()
        {
            return MODEL_PATH;
        }
        public static string GetUIFXPath(string assetName)
        {
            return UI_FX_PATH + assetName;
        }
        public static string GetRawPath(string path)
        {
            return RAW_STREAMING_ASSETS + "/" + path;
        }

        public static string GetPersistentPath(string path)
        {
            return string.Format(cStreamingAssetsPath, PERSISTENT_STREAMING_ASSETS, path);
        }

        public static string GetPersistentFilePath(string path)
        {
            return string.Format(cStreamingAssetsPath, Application.persistentDataPath, path);
        }

        public static bool CheckPersistentFileExists(string path)
        {
            return File.Exists(GetPersistentFilePath(path));
        }

        public static bool CheckRelease()
        {
            return false;
        }

        public static string GetAssetPath(string path)
        {
            if (CheckRelease())
            {
                return GetPersistentPath(path);
            }

            return GetRawPath(path);
        }

        public static string GetAssetFilePath(string path)
        {
            if (CheckRelease())
            {
                return GetPersistentFilePath(path);
            }

            return GetRawFilePath(path);
        }

        public static string GetWWWAssetPath(string path)
        {
            if (CheckRelease())
            {
                return GetPersistentPath(path);
            }

            return GetRawPath(path);
        }

    }
}
