using Assets.Scripts.Com.Game.Utils;
using Com.Game.Core;
using Com.Game.Manager;
using Com.Manager.AssetLoader;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
namespace Com.Game.Manager
{
    static public class SyncResourceManager
    {
        private const int mAudioClipType = 0;
        private const int mUIType = 1;
        private const int mUIFxType = 2;
        private const int mUITexture = 3;
        private const int mExternalGo = 4;
        private const int mExternalTexture = 5;
        private const int mExternalRole = 6;

        public delegate void mInitCallback();
        public delegate void mInitCallback<T>(T arg);

        public delegate void CallBackDelegate();
        public delegate void CallBackDelegate<T>(T arg);
        public delegate void CallBackDelegate<T1,T2>(T1 arg1,T2 arg2);
        // public delegate void CallBackDelegate<T>(T arg);

        static CallBackDelegate mCallBackDelegate;
        // static private LuaFunction mInitCallback;
        static private AssetBundleManifest mManifest;
        static private AssetBundleManifest mExternalManifest;
        public delegate void LoadFunc(Bundle bundle, string objectName, CallBackDelegate<object, object> callBack, string assetBundlePath = "");
        public delegate string[] GetDependBundlesFunc(string assetBundleName);
        public delegate string GetPathFunc(string assetBundleName);
        static async public Task Init(CallBackDelegate callBack)
        {
            mCallBackDelegate = callBack;
            await InitSub();
            return;
        }
        static private async Task InitSub()
        {
            await LoadManifest();
            return;
        }
        static private async Task LoadManifest()
        {
            string streamingAssetsStr = "StreamingAssets";
            string assetBundleManifestStr = "AssetBundleManifest";
            WWW www = new WWW(ResourceURL.GetUrl(streamingAssetsStr));
            await new WaitUntil(() => www.isDone);
            AssetBundle manifestBundle = www.assetBundle;
            mManifest = (AssetBundleManifest)manifestBundle.LoadAsset(assetBundleManifestStr, typeof(AssetBundleManifest));
            manifestBundle.Unload(false);
            www.Dispose();
            //TODO ADD ART WORK SPACE
            
            WWW www1 = new WWW(ResourceURL.GetExternalUrl(streamingAssetsStr));
            await new WaitUntil(() => www1.isDone);
            AssetBundle manifestBundle1 = www1.assetBundle;
            mExternalManifest = (AssetBundleManifest)manifestBundle1.LoadAsset(assetBundleManifestStr, typeof(AssetBundleManifest));
            manifestBundle1.Unload(false);
            www1.Dispose();
            mCallBackDelegate();
            //  mInitCallback.Call();
            mCallBackDelegate = null;
        }
        static string[] GetAllDependencies(string assetBundleName)
        {
            return mManifest.GetAllDependencies(assetBundleName);
        }
        static string[] GetAllDependenciesExternal(string assetBundleName)
        {
            return mExternalManifest.GetAllDependencies(assetBundleName);
        }
        static string GetPath(string assetName)
        {
            return assetName;
        }

        static void InternalLoadScene(string file, CallBackDelegate<object, object> callBack, GetPathFunc getPathFunc, GetDependBundlesFunc getDependBundlesFunc, bool exernalScene)
        {
            AssetLoaderManager.Instance.LoadAsset(ResourceURL.GetScenePath(), file, callBack, getPathFunc, getDependBundlesFunc, new LoadFunc(LoadSceneFunc), exernalScene);
        }

        static void InternalLoadAsset(string path, string file, CallBackDelegate<object, object> callBack, GetPathFunc getPathFunc, GetDependBundlesFunc getDependBundlesFunc, LoadFunc loadFunc)
        {
            AssetLoaderManager.Instance.LoadAsset(path, file, callBack, getPathFunc, getDependBundlesFunc, loadFunc);
        }

        static public void PreLoadAssetExternal(string path, string file, CallBackDelegate<object, object> callBack)
        {
            if (AsyncResourceManager.CheckLoaded(ResourceURL.GetExternalPath(path + file)) || AsyncResourceManager.CheckAssetBundleLoaderAsync(ResourceURL.GetExternalPath(path + file)))
            {
             //   AsyncResourceManager.Instance.LoadExternalAsync(path, file, callBack);
                return;
            }
            SyncResourceManager.InternalLoadAsset(path, file, callBack, ResourceURL.GetExternalPath, SyncResourceManager.GetAllDependenciesExternal, SyncResourceManager.PreLoadAssetFunc); 
        }

        static public AssetBundle GetBundle(string bundlePath,bool isMainBundle)
        {
            return AssetLoaderManager.Instance.GetBundle(bundlePath, isMainBundle).mBundle;
        }

        static void PreLoadAssetFunc(Bundle bundle, string objectName, CallBackDelegate<object, object> callBack, string assetBundlePath = "")
        {
            if (bundle!=null)
            {
                bundle.PreLoadFunc();
            }
        }

        static public void PreLoadAsset(string path, string file, CallBackDelegate<object, object> callBack)
        {
            SyncResourceManager.InternalLoadAsset(path, file, callBack, SyncResourceManager.GetPath, SyncResourceManager.GetAllDependencies, SyncResourceManager.PreLoadAssetFunc);
        }

        static public void SetLoadAllFinishCallBack(CallBackDelegate<object, object> callBack)
        {
            AssetLoaderManager.Instance.SetLoadAllFinishCallBack(callBack);
        }

        static public bool CheckSyncAssetManagerLoaded(string path)
        {
            if (!AssetLoaderManager.Instance.mLoadedBunders.ContainsKey(path))
            {
                return false;
            }
            return true;
        }
        static public void LoadScene(string sceneName, CallBackDelegate<object, object> callBack)
        {
            SyncResourceManager.InternalLoadScene(sceneName, callBack, SyncResourceManager.GetPath, SyncResourceManager.GetAllDependencies, false);
        }

        static public void UnityLoadScene(string sceneName, CallBackDelegate<object, object> callBack)
        {
            SceneManager.LoadScene(sceneName);
            if (callBack != null)
            {
                callBack(null,null);
               // callBack.Call();
            }
        }

        static public void LoadSceneExternal(string sceneName, CallBackDelegate<object, object> callBack)
        {
            SyncResourceManager.InternalLoadScene(sceneName, callBack, ResourceURL.GetExternalPath, SyncResourceManager.GetAllDependenciesExternal, true);
        }

        static public void LoadObjectExternal(string path, string file, CallBackDelegate<object, object> callBack)
        {
            if (AsyncResourceManager.CheckLoaded(ResourceURL.GetExternalPath(path + file))|| AsyncResourceManager.CheckAssetBundleLoaderAsync(ResourceURL.GetExternalPath(path + file)))
            {
                AsyncResourceManager.LoadExternalAsync(path, file, callBack);
                return;
            }
            SyncResourceManager.InternalLoadAsset(path, file, callBack, ResourceURL.GetExternalPath, SyncResourceManager.GetAllDependenciesExternal, SyncResourceManager.LoadExternalGoFunc);
        }

        static public void LoadRoleExternal(string file, CallBackDelegate<object, object> callBack)
        {
            SyncResourceManager.InternalLoadAsset(ResourceURL.GetModelPath(), file, callBack, ResourceURL.GetExternalPath, SyncResourceManager.GetAllDependenciesExternal, SyncResourceManager.LoadExternalRoleFunc);
        }

        static public void LoadTextureExternal(string path, string file, CallBackDelegate<object, object> callBack)
        {
            SyncResourceManager.InternalLoadAsset(path, file, callBack, ResourceURL.GetExternalPath, SyncResourceManager.GetAllDependenciesExternal, SyncResourceManager.LoadExternalTextureFunc);
        }

        static public void LoadUIFXExternal(string file, CallBackDelegate<object, object> callBack)
        {
            SyncResourceManager.InternalLoadAsset(ResourceURL.GetUIFXPath(""), file, callBack, ResourceURL.GetExternalPath, SyncResourceManager.GetAllDependenciesExternal, SyncResourceManager.LoadUIFxObjectFunc);
        }

        static public void LoadUI(string path, string file, CallBackDelegate<object, object> callBack)
        {
            SyncResourceManager.InternalLoadAsset(path, file, callBack, SyncResourceManager.GetPath, SyncResourceManager.GetAllDependencies, SyncResourceManager.LoadUIObjectFunc);
        }

        static public void LoadTexture(string path, string file, CallBackDelegate<object, object> callBack)
        {
            SyncResourceManager.InternalLoadAsset(path, file, callBack, SyncResourceManager.GetPath, SyncResourceManager.GetAllDependencies, SyncResourceManager.LoadUIIconObjectFunc);
        }

        static public void LoadAudioClip(string path, string file, CallBackDelegate<object, object> callBack)
        {
            SyncResourceManager.InternalLoadAsset(path, file, callBack, SyncResourceManager.GetPath, SyncResourceManager.GetAllDependencies, SyncResourceManager.LoadUIIconObjectFunc);
        }

        static public void UnloadBundles()
        {
            AssetLoaderManager.Instance.UnloadBundles();
        }

        static public int  GetNowTaskNum()
        {
            return AssetLoaderManager.Instance.mTask.Count;
        }

        static async void LoadSceneFunc(Bundle bundle, string objectName, CallBackDelegate<object, object> callBack, string assetBundlePath = "")
        {
            SceneManager.LoadScene(objectName);
            await new WaitUntil(() => SceneManager.GetActiveScene().name == objectName);
            if (callBack != null)
            {
                callBack(null,null);
               // callBack.Call();
            }
        }

        static void LoadExternalGoFunc(Bundle bundle, string objectName, CallBackDelegate<object, object> callBack, string assetBundlePath = "")
        {
            Debug.Log("load" + objectName);
            bundle.LoadObject(objectName, callBack, true, mExternalGo);
            if (assetBundlePath != "")
            {
                // if (!AsyncResourceManager.Instance.CheckObject(assetBundlePath))
                // bundle.LoadObject(objectName, callBack, true, mExternalGo);
            }
            else
            {

            }
        }
        static void LoadExternalRoleFunc(Bundle bundle, string objectName, CallBackDelegate<object, object> callBack, string assetBundlePath = "")
        {
            bundle.LoadObject(objectName, callBack, true, mExternalRole);
        }
        static void LoadExternalTextureFunc(Bundle bundle, string objectName, CallBackDelegate<object, object> callBack, string assetBundlePath = "")
        {
            bundle.LoadObject(objectName, callBack, false, mExternalTexture);
        }
        static void LoadUIFxObjectFunc(Bundle bundle, string objectName, CallBackDelegate<object, object> callBack, string assetBundlePath = "")
        {
            bundle.LoadObject(objectName, callBack, true, mUIFxType);
        }
        static void LoadUIObjectFunc(Bundle bundle, string objectName, CallBackDelegate<object, object> callBack, string assetBundlePath = "")
        {
            Debug.Log("loadUI" + objectName);
            bundle.LoadObject(objectName, callBack, true, mUIType);
        }
        static void LoadUIIconObjectFunc(Bundle bundle, string objectName, CallBackDelegate<object, object> callBack, string assetBundlePath = "")
        {
            bundle.LoadObject(objectName, callBack, false, mUITexture);
        }
        static void LoadAudioClipObjectFunc(Bundle bundle, string objectName, CallBackDelegate<object, object> callBack, string assetBundlePath = "")
        {
            bundle.LoadObject(objectName, callBack, false, mAudioClipType);
        }
        static public void OnUpdate()
        {
            AssetLoaderManager.Instance.Update();
        }
    }
}
