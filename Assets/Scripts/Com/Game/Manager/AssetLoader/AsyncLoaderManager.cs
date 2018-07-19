using Com.Game.Core;
using Com.Game.Manager;
using Com.Manager.AssetLoader;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using static Com.Game.Manager.AsyncResourceManager;
using static Com.Game.Manager.SyncResourceManager;

public class AsyncLoaderManager : Singleton<AsyncLoaderManager>
{
    private readonly QueueDictionary<string, ABInfo> cacheDictionary = new QueueDictionary<string, ABInfo>();
    private readonly Dictionary<string, UnityEngine.Object> resourceCache = new Dictionary<string, UnityEngine.Object>();
    private readonly List<AssetsBundleLoaderAsync> mAssetsBundleLoaderAsync = new List<AssetsBundleLoaderAsync>();
    private readonly List<AssetsLoaderAsync> mAssetsLoaderAsync = new List<AssetsLoaderAsync>();
    private readonly Dictionary<string, AssetsLoaderAsync> mAssetsLoaderAsyncDic = new Dictionary<string, AssetsLoaderAsync>();
    public readonly Dictionary<string, ABInfo> bundles = new Dictionary<string, ABInfo>();
    public delegate void DelegateLoadExternalMehod(string path, string file, CallBackDelegate<object, object> callBack, GetDependBundlesFuncDelegate getExternalDependcesFunc);
    public delegate void DelegateLoadBundleMehod(string path, string file, CallBackDelegate<object, object> callBack);
    List<string> taskList = new List<string>();
    List<string> removeList = new List<string>();
    List<string> TestTaskList = new List<string>();

    public void LoadExternalAsync(string path, string file, CallBackDelegate<object, object> callBack, GetPathFuncDelegate getPathFunc, GetDependBundlesFuncDelegate getDependcesFunc)
    {
        this.InternalLoadExternalAssetAsync(path, file, callBack, new DelegateLoadExternalMehod(this.SetExternal), getPathFunc, getDependcesFunc);
    }
    public void LoadTextTureAsync(string path, string file, CallBackDelegate<object, object> callBack, GetPathFuncDelegate getPathFunc, GetDependBundlesFuncDelegate getDependcesFunc)
    {
        this.InternalLoadAssetAsync(path, file, callBack, new DelegateLoadBundleMehod(this.SetTexture), getPathFunc, getDependcesFunc);
    }

    public async void InternalLoadExternalAssetAsync(string path, string file, CallBackDelegate<object, object> callBack, DelegateLoadExternalMehod method, GetPathFuncDelegate getExternalPathFunc, GetDependBundlesFuncDelegate getExternalDependcesFunc)
    {
        string assetBundleName = path + file;
        string assetBundlePath = getExternalPathFunc(assetBundleName);
        string[] dependencies = getExternalDependcesFunc(assetBundleName);
        if (SyncResourceManager.CheckSyncAssetManagerLoaded("external/" + assetBundleName))
        {
         //   Debug.Log(assetBundleName);
            SyncResourceManager.LoadObjectExternal(path, file, callBack);
            return;
        }

        if (taskList.Contains("external/" + assetBundleName))
        {
            await new WaitUntil(() => !this.taskList.Contains("external/" + assetBundleName));
        }
        taskList.Add("external/" + assetBundleName);
        if (!this.bundles.ContainsKey("external/" + assetBundleName) && !this.resourceCache.ContainsKey("external/" + assetBundleName))
        {
            foreach (string dependency in dependencies)
            {
                if (string.IsNullOrEmpty(dependency))
                {
                    continue;
                }
                if (!SyncResourceManager.CheckSyncAssetManagerLoaded("external/" + dependency))
                {
                    if (!TestTaskList.Contains("external/" + dependency) && !this.bundles.ContainsKey("external/" + dependency))
                    {
                        await this.MyLoadOneBundleAsync("external/" + dependency, path);
                    }
                    else if (TestTaskList.Contains("external/" + dependency))
                    {
                        await new WaitUntil(() => this.bundles.ContainsKey("external/" + dependency));
                    }
                }
            }
        }
        foreach (string dependency in dependencies)
        {
            if (string.IsNullOrEmpty(dependency))
            {
                continue;
            }
            if (!SyncResourceManager.CheckSyncAssetManagerLoaded("external/" + dependency))
            {
                await new WaitUntil(() => this.bundles.ContainsKey("external/" + dependency));
            }
        }
        method(path, file, callBack, getExternalDependcesFunc);
        taskList.Remove("external/" + assetBundleName);
    }
    public async void InternalLoadAssetAsync(string path, string file, CallBackDelegate<object, object> callBack, DelegateLoadBundleMehod method, GetPathFuncDelegate getExternalPathFunc, GetDependBundlesFuncDelegate getExternalDependcesFunc)
    {
        string assetBundleName = path + file;
        string assetBundlePath = getExternalPathFunc(assetBundleName);
        string[] dependencies = getExternalDependcesFunc(assetBundleName);
        if (taskList.Contains(assetBundleName))
        {
            await new WaitUntil(() => !this.taskList.Contains(assetBundleName));
        }
        taskList.Add(assetBundleName);
        if (!this.bundles.ContainsKey(assetBundleName))
        {
            foreach (string dependency in dependencies)
            {
                if (string.IsNullOrEmpty(dependency))
                {
                    continue;
                }
                if (!this.bundles.ContainsKey(dependency) && !TestTaskList.Contains(dependency))
                    await this.MyLoadOneBundleAsync(dependency, path);
                else if (TestTaskList.Contains(dependency))
                {
                    await new WaitUntil(() => this.bundles.ContainsKey(assetBundleName) == true);
                }
            }
        }
        foreach (string dependency in dependencies)
        {
            await new WaitUntil(() => this.bundles.ContainsKey(dependency) == true);
        }
        method(path, file, callBack);
        taskList.Remove(assetBundleName);
    }
    private async Task MyLoadOneBundleAsync(string assetBundleName, string path)
    {
        ABInfo abInfo;
        if (this.bundles.TryGetValue(assetBundleName, out abInfo))
        {
            return;
        }
        AssetBundle assetBundle;
        TestTaskList.Add(assetBundleName);
        Debug.Log(assetBundleName);
        AssetsBundleLoaderAsync bundleLoaderAsync = new AssetsBundleLoaderAsync();
        mAssetsBundleLoaderAsync.Add(bundleLoaderAsync);
        assetBundle = await bundleLoaderAsync.LoadAsync(assetBundleName, path);
        this.bundles[assetBundleName] = new ABInfo(assetBundleName, assetBundle);
        TestTaskList.Remove(assetBundleName);
    }
    public void UnloadBundles()
    {
        // Debug.Log("UNDLOAD  " + bundles.Count);
        foreach (var v in this.bundles)
        {
            // Debug.Log(v.Key);
            // Debug.Log(v.Value.RefCount);
            if (v.Value.IfTexture == true)
                return;
            if (v.Value.CheckRefObj() == true)
            {
                Debug.Log("Dispose" + v.Value.Name);
                v.Value.Dispose();
                this.resourceCache.Remove(v.Key);
                removeList.Add(v.Key);
            }

        }
        foreach (var v in removeList)
        {
            this.bundles.Remove(v);
        }
        removeList.Clear();
        //this.bundles.Clear();
        //this.resourceCache.Clear();
    }
    public bool CheckLoaded(string path)
    {
        string assetBundleName = path;
        if (this.cacheDictionary.ContainsKey(assetBundleName) || this.bundles.ContainsKey(assetBundleName) || this.bundles.ContainsKey("external/" + assetBundleName) || this.cacheDictionary.ContainsKey("external/" + assetBundleName))
        {
            return true;
        }
        else
            return false;
    }
    public bool CheckObject(string name)
    {
        if (this.resourceCache.ContainsKey(name) || this.bundles.ContainsKey(name))
            return true;
        return false;
    }
    public bool CheckAssetBundleLoaderAsync(string path)
    {
        //  Debug.Log(TestTaskList.Contains(path) +"   "+ path);
        if (TestTaskList.Contains(path))
            return true;
        else
            return false;
    }
    public void OnUpdate()
    {
        for (int i = mAssetsBundleLoaderAsync.Count - 1; i >= 0; i--)
        {
            mAssetsBundleLoaderAsync[i].Update();
            if (mAssetsBundleLoaderAsync[i].tag)
            {
                mAssetsBundleLoaderAsync[i].Dispose();
                mAssetsBundleLoaderAsync.Remove(mAssetsBundleLoaderAsync[i]);
            }
        }
        for (int i = mAssetsLoaderAsync.Count - 1; i >= 0; i--)
        {
            mAssetsLoaderAsync[i].Update();
            if (mAssetsLoaderAsync[i].tag)
            {
                mAssetsLoaderAsyncDic.Remove(mAssetsLoaderAsync[i].ReturnAssetBundleName());
                mAssetsLoaderAsync[i].Dispose();
                mAssetsLoaderAsync.Remove(mAssetsLoaderAsync[i]);
            }
        }
    }
    public AssetBundle GetBundle(string bundleName)
    {
        if (this.bundles.ContainsKey(bundleName))
            return this.bundles[bundleName].AssetBundle;
        else
            return null;
    }

    public async void SetExternal(string path, string file, CallBackDelegate<object, object> mCall, GetDependBundlesFuncDelegate getExternalDependcesFunc)
    {
        string assetName = path + file;
        string[] dependencies = getExternalDependcesFunc(assetName);
        if (this.mAssetsLoaderAsyncDic.ContainsKey("external/" + assetName))
        {
            Debug.Log("await");
            await new WaitUntil(() => mAssetsLoaderAsyncDic.ContainsKey("external/" + assetName) == false && this.resourceCache.ContainsKey("external/" + assetName));
        }
        if (this.resourceCache.ContainsKey("external/" + assetName))
        {
          //  Debug.Log(("external/" + assetName));
            GameObject obj = GameObject.Instantiate((GameObject)this.resourceCache["external/" + assetName]);
            this.bundles["external/" + assetName].AddRefObj(obj);
            // Debug.Log("AddRef" + "external/" + assetName + "Num" + this.bundles["external/" + assetName].RefCount);
            foreach (var v in dependencies)
            {
                if (v != assetName)
                {
                    if (!SyncResourceManager.CheckSyncAssetManagerLoaded("external/" + v))
                    {
                        this.bundles["external/" + v].AddRefObj(obj);
                        //   Debug.Log("AddRef" + "external/" + v + "Num" + this.bundles["external/" + v].RefCount);
                    }
                }
            }
            mCall(obj, file);
           // mLua.Call(obj, file);
        }
        else
        {
            AssetsLoaderAsync assetsLoaderAsync = new AssetsLoaderAsync();
            assetsLoaderAsync.SetBundle(this.bundles["external/" + assetName].AssetBundle, "external/" + assetName);
            mAssetsLoaderAsync.Add(assetsLoaderAsync);
            mAssetsLoaderAsyncDic["external/" + assetName] = assetsLoaderAsync;
            GameObject temp = (GameObject)await assetsLoaderAsync.LoadAssetAsync(file);
            GameObject obj = GameObject.Instantiate(temp);
            this.bundles["external/" + assetName].AddRefObj(obj);
            //   Debug.Log("AddRef" + "external/" + assetName + "Num" + this.bundles["external/" + assetName].RefCount);
            foreach (var v in dependencies)
            {
                if (v != assetName)
                {
                    if (!SyncResourceManager.CheckSyncAssetManagerLoaded("external/" + v))
                    {
                        this.bundles["external/" + v].AddRefObj(obj);
                        //    Debug.Log("AddRef" + "external/" + v + "Num" + this.bundles["external/" + v].RefCount);
                    }
                }
            }
            this.bundles["external/" + assetName].AssetBundle.Unload(false);
            this.resourceCache["external/" + assetName] = temp;
            mCall(obj, file);
            //mLua.Call(obj,file);
        }
    }
    public async void SetTexture(string path, string file, CallBackDelegate<object, object> mCall)
    {
        string assetName = path + file;
        if (this.mAssetsLoaderAsyncDic.ContainsKey(assetName))
        {
            await new WaitUntil(() => mAssetsLoaderAsyncDic.ContainsKey(assetName) == false && this.resourceCache.ContainsKey(assetName));
        }
        if (this.resourceCache.ContainsKey(assetName))
        {
            System.Type mType = this.resourceCache[assetName].GetType();
            if (mType == typeof(Sprite))
            {
                Sprite sprite = (Sprite)this.resourceCache[assetName];
                mCall(sprite.texture,null);
                //mLua.Call(sprite.texture);
                return;
            }
            mCall((Texture)this.resourceCache[assetName], null);
           // mLua.Call((Texture)this.resourceCache[assetName]);
        }
        else
        {
            AssetsLoaderAsync assetsLoaderAsync = new AssetsLoaderAsync();
            assetsLoaderAsync.SetBundle(this.bundles[path + file].AssetBundle, path + file);
            mAssetsLoaderAsync.Add(assetsLoaderAsync);
            mAssetsLoaderAsyncDic[path + file] = assetsLoaderAsync;
            UnityEngine.Object obj = await assetsLoaderAsync.LoadAssetAsync(file);
            this.resourceCache[assetName] = obj;
            System.Type mType = obj.GetType();
            this.bundles[path + file].IfTexture = true;
            if (mType == typeof(Sprite))
            {
                Sprite sprite = (Sprite)obj;
                mCall(sprite.texture, null);
              //  mLua.Call(sprite.texture);
                return;
            }
            mCall((Texture)obj, null);
           // mLua.Call((Texture)obj);
        }
    }
}
