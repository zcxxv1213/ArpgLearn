using Com.Game.Core;
using Com.Game.Manager;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Com.Game.Manager.SyncResourceManager;

namespace Com.Manager.AssetLoader
{
    public class AssetLoaderManager: Singleton<AssetLoaderManager>
    {
        int mMaxLoadCount = 1;
        int mCurLoadCount = 0;
        bool mIsMobilePlatform = !Application.isEditor;

        public Dictionary<string, Bundle> mMainBundles = new Dictionary<string, Bundle>();
        public Dictionary<string, Bundle> mDependBundles = new Dictionary<string, Bundle>();
        public Dictionary<string, Bundle> mAllBundles = new Dictionary<string, Bundle>();
        public Dictionary<string, AssetBundle> mLoadedBunders = new Dictionary<string, AssetBundle>();
        public Dictionary<Bundle, LoadTask> mBundleLoadTaskList = new Dictionary<Bundle, LoadTask>();
        public List<LoadTask> mTask = new List<LoadTask>();
        CallBackDelegate<object, object> mLoadAllFinishCallBack;

        void Init()
        {

        }
        public void UnloadBundles()
        {
            foreach (var v in this.mMainBundles)
            {
                v.Value.CheckUnload();
            }
            foreach (var v in this.mDependBundles)
            {
                v.Value.CheckUnload();
            }
        }
        public Bundle GetBundle(string bundlePath, bool isMainBundle = false)
        {
            if (!this.mAllBundles.ContainsKey(bundlePath))
            {
                Bundle bundle;
                if (isMainBundle)
                {
                    bundle = new MainBundle(bundlePath);
                    this.mMainBundles[bundlePath] = bundle;
                }
                else
                {
                    bundle = new Bundle(bundlePath);
                    this.mDependBundles[bundlePath] = bundle; 
                }
                this.mAllBundles[bundlePath] = bundle;
            }
            return this.mAllBundles[bundlePath];
        }
        public void SetLoadAllFinishCallBack(CallBackDelegate<object, object> callBack)
        {
            if (this.mTask.Count == 0)
            {
                callBack(null,null);
          //      callBack.Call();
                return;
            }
            mLoadAllFinishCallBack = callBack;
        }
        private Bundle[] GetDependBundles(string assetBundleName, GetPathFunc getPathFunc, GetDependBundlesFunc getDependenciesFunc)
        {
            string[] dependencies = getDependenciesFunc(assetBundleName);
            Bundle[] dependBundles = new Bundle[dependencies.Length];
            if (dependencies.Length > 0)
            {
                int i = 0;
                foreach (var v in dependencies)
                {
                    dependBundles[i] = this.GetBundle(getPathFunc(dependencies[i]));
                    i++;
                }
            }
            return dependBundles;
        }
        public void LoadAsset(string path, string fileName, CallBackDelegate<object, object> callBack, GetPathFunc getPathFunc, GetDependBundlesFunc getDependenciesFunc,LoadFunc loadAssetFunc,bool externalScene = false)
        {
            string assetBundleName = path + fileName;
            string assetBundlePath = getPathFunc(assetBundleName);
            Bundle bundle = this.GetBundle(assetBundlePath, true);
            bundle.mExternalScene = externalScene && mIsMobilePlatform;
            if (mBundleLoadTaskList.ContainsKey(bundle))
            {
                mBundleLoadTaskList[bundle].AddParams(fileName, callBack, loadAssetFunc);
                return;
            }
            if (bundle.mLoadCompleted)
            {
                bundle.LoadParams(fileName, callBack, loadAssetFunc);
                return;
            }
            if (mIsMobilePlatform && callBack != null)
            {
                bundle.mPreloadAssetName = fileName;
            }
            Bundle[] dependBundles = this.GetDependBundles(assetBundleName, getPathFunc, getDependenciesFunc);
            bundle.SetDependBundles(dependBundles);
            this.mLoadedBunders[assetBundlePath] = bundle.mBundle;
            LoadTask loadTask = new LoadTask(bundle, dependBundles);
            loadTask.AddParams(fileName, callBack, loadAssetFunc);
            mBundleLoadTaskList[bundle] = loadTask;
            mTask.Insert(0, loadTask);
            //mTask.Add(  loadTask );
            this.StartLoadTask();
        }

        public void Update()
        {
            int taskCount = this.mTask.Count;
            if (taskCount > 0)
            {
                bool startLoad = false;
                LoadTask l_task = null;

                for (int i = taskCount -1; i >= 0; i--)
                {
                    l_task = this.mTask[i];
                    if (l_task.CheckLLoad())
                    {
                        startLoad = true;
                        this.mCurLoadCount = this.mCurLoadCount - l_task.mLoadCount;

                        this.mBundleLoadTaskList.Remove(l_task.mMainBundle);
    
                        //以下两行代码顺序不能改
                        this.mTask.Remove(l_task);
                        l_task.LoadComplete();
                    }
                    else
                    {
                        if (startLoad)
                        {
                            this.StartLoadTask();
                        }
                        return;
                    }
                }
                if (startLoad)
                {
                    this.StartLoadTask();
                }
            }
        }

        private void StartLoadTask()
        {
            int taskCount = this.mTask.Count;
            int curCount = mCurLoadCount;
            if (taskCount > 0)
            {
                if (curCount < mMaxLoadCount)
                {
                    for (int i = taskCount -1; i >= 0; i--)
                    {
                        if (mTask[i].mLoadCount == 0)
                        {
                            curCount = curCount + mTask[i].StartLoad();

                            if (curCount >= mMaxLoadCount)
                            {
                                mCurLoadCount = curCount;
                                return;
                            }
                        }
                    }
                }
                mCurLoadCount = curCount;
            }
            else
            {
                if (this.mLoadAllFinishCallBack != null)
                {
                    this.mLoadAllFinishCallBack(null,null);
                    //this.mLoadAllFinishCallBack.Call();
                    this.mLoadAllFinishCallBack = null;
                }
            }
        }
    }
}