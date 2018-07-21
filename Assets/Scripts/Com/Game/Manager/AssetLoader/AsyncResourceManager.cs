using Assets.Scripts.Com.Game.Utils;
using Com.Game.Core;
using Com.Game.Manager;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using static Com.Game.Manager.SyncResourceManager;

namespace Com.Game.Manager
{
    static public class AsyncResourceManager
    {
        static string EXTERNAL_PATH = "external/";
        
        static AssetBundleManifest mBundleManifest;
        static AssetBundleManifest mExternalBundleManifest;
        static string assetBundleManifestStr = "AssetBundleManifest";

        public delegate string[] GetDependBundlesFuncDelegate(string assetBundleName);
        public delegate string GetPathFuncDelegate(string assetBundleName);
        static async public Task Init()
        {
            await LoadManifest(ResourceURL.GetUrl("StreamingAssets"));
            await LoadExternaManifest(ResourceURL.GetExternalUrl("StreamingAssets"));
            return;
        }

        static public async Task LoadManifest(string streamingAssetsStr)
        {
            WWW bundle = new WWW(streamingAssetsStr);
            await LoadBundleManifest(bundle,false);
            return;
        }

        static public async Task LoadExternaManifest(string streamingAssetsStr)
        {
            WWW externalBundle = new WWW(streamingAssetsStr);
            await LoadBundleManifest(externalBundle, true);
            return ;
        }
        static async Task LoadBundleManifest(WWW bundle, bool ifExternal)
        {
            await new WaitUntil(() => bundle.isDone);
            AssetBundle mBundle = bundle.assetBundle;
            if (ifExternal)
            {
                mExternalBundleManifest = (AssetBundleManifest)mBundle.LoadAsset(assetBundleManifestStr, typeof(AssetBundleManifest));
            }
            else
            {
                mBundleManifest = (AssetBundleManifest)mBundle.LoadAsset(assetBundleManifestStr, typeof(AssetBundleManifest));
            }
            mBundle.Unload(false);
            bundle.Dispose();
        }
        static private string GetExternalPath(string assetName)
        {
            return EXTERNAL_PATH + assetName;
        }
        /* public void LoadExternalAsync(string path, string file, LuaFunction callBack, LuaFunction checkFunc, LuaFunction luaLoadFunc)
         {
             this.InternalLoadExternalAssetAsync(path, file, callBack, new DelegateMethod(this.SetExternal), checkFunc, luaLoadFunc);
         }*/

        static public void LoadExternalAsync(string path, string file, CallBackDelegate<object, object> callBack)
        {
            AsyncLoaderManager.Instance.LoadExternalAsync(path, file, callBack, AsyncResourceManager.GetExternalPathFunc,AsyncResourceManager.GetAllExternalDependenciesFunc);
        }

        static public void LoadTextTureAsync(string path, string file, CallBackDelegate<object, object> callBack)
        {
            AsyncLoaderManager.Instance.LoadTextTureAsync(path, file, callBack, AsyncResourceManager.GetPathFunc, AsyncResourceManager.GetAllDependenciesFunc);
            //InternalLoadAssetAsync(path, file, callBack, new DelegateMethodWithoutCheck(SetTexture));
        }

        static string[] GetAllExternalDependenciesFunc(string assetBundleName)
        {
            return GetSortedExternalDependencies(assetBundleName);
        }
        static string[] GetAllDependenciesFunc(string assetBundleName)
        {
            return GetSortedDependencies(assetBundleName);
        }

        static string GetExternalPathFunc(string assetBundleName)
        {
            return GetExternalPath(assetBundleName);
        }
        static string GetPathFunc(string assetBundleName)
        {
            return assetBundleName;
        }
        static public AssetBundle GetBundle(string bundleName)
        {
            return AsyncLoaderManager.Instance.GetBundle(bundleName);
        }

        static public bool CheckAssetBundleLoaderAsync(string path)
        {
            return AsyncLoaderManager.Instance.CheckAssetBundleLoaderAsync(path);
        }
        static public void UnloadBundles()
        {
            AsyncLoaderManager.Instance.UnloadBundles();
        }
        
        static public bool CheckLoaded(string path)
        {
            return AsyncLoaderManager.Instance.CheckLoaded(path);
        }
        static public bool CheckObject(string name)
        {
            return AsyncLoaderManager.Instance.CheckObject(name);
        }
       static public void OnUpdate()
        {
            AsyncLoaderManager.Instance.OnUpdate(); 
        }

        #region GetDependency
        static private string[] GetSortedDependencies(string assetBundleName)
        {
            Dictionary<string, int> info = new Dictionary<string, int>();
            List<string> parents = new List<string>();
            CollectDependencies(parents, assetBundleName, info);
            string[] ss = info.OrderBy(x => x.Value).Select(x => x.Key).ToArray();
            return ss;
        }
        static private string[] GetSortedExternalDependencies(string assetBundleName)
        {
            Dictionary<string, int> info = new Dictionary<string, int>();
            List<string> parents = new List<string>();
            CollectExternalDependencies(parents, assetBundleName, info);
            string[] ss = info.OrderBy(x => x.Value).Select(x => x.Key).ToArray();
            return ss;
        }
        static private void CollectExternalDependencies(List<string> parents, string assetBundleName, Dictionary<string, int> info)
        {
            parents.Add(assetBundleName);
            string[] deps = GetExternalDependencies(assetBundleName);
            foreach (string parent in parents)
            {
                if (!info.ContainsKey(parent))
                {
                    info[parent] = 0;
                }
                info[parent] += deps.Length;
            }


            foreach (string dep in deps)
            {
                if (parents.Contains(dep))
                {
                    Debug.Log("包有循环依赖，请重新标记:" + assetBundleName);
                }
                CollectExternalDependencies(parents, dep, info);
            }
            parents.RemoveAt(parents.Count - 1);
        }
        static private void CollectDependencies(List<string> parents, string assetBundleName, Dictionary<string, int> info)
        {
            parents.Add(assetBundleName);
            string[] deps = GetDependencies(assetBundleName);
            foreach (string parent in parents)
            {
                if (!info.ContainsKey(parent))
                {
                    info[parent] = 0;
                }
                info[parent] += deps.Length;
            }

            foreach (string dep in deps)
            {
                if (parents.Contains(dep))
                {
                    Debug.Log("包有循环依赖，请重新标记:" + assetBundleName);
                }
                CollectDependencies(parents, dep, info);
            }
            parents.RemoveAt(parents.Count - 1);
        }
        static private string[] GetExternalDependencies(string assetBundleName)
        {
            string[] dependencies = mExternalBundleManifest.GetAllDependencies(assetBundleName);
            return dependencies;
        }
        static private string[] GetDependencies(string assetBundleName)
        {
            // dependencies = new string[0];
            string[] dependencies = mBundleManifest.GetAllDependencies(assetBundleName);
            return dependencies;
        }
#endregion
    }
}
