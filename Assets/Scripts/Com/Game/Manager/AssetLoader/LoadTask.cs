using Com.Game.Manager;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Com.Game.Manager.SyncResourceManager;

namespace Com.Manager.AssetLoader
{
    public class LoadTask
    {
        public MainBundle mMainBundle;
        Bundle[] mDependBundles;
        public int mLoadCount = 0;
        Queue<Params> mParamsList = new Queue<Params>();
        struct Params
        {
            public string objectName;
            public CallBackDelegate<object, object> callBack;
            public LoadFunc loadObjectFunc;
        }
        public LoadTask(Bundle mainBundle,Bundle[] dependBundles)
        {
            mMainBundle = (MainBundle)mainBundle;
            mDependBundles = dependBundles;
        }
        public void AddParams(string objectName, CallBackDelegate<object, object> callBack, LoadFunc loadObjectFunc = null)
        {
            Params mParams = new Params();
            mParams.objectName = objectName;
            mParams.callBack = callBack;
            mParams.loadObjectFunc = loadObjectFunc;
            this.mParamsList.Enqueue(mParams);
        }
        private void LoadParams(string objectName, CallBackDelegate<object, object> callBack, LoadFunc loadObjectFunc = null)
        {
            mMainBundle.LoadParams(objectName, callBack, loadObjectFunc);
        }
        public void LoadComplete()
        {
            foreach (var v in this.mParamsList)
            {
                this.LoadParams(v.objectName, v.callBack, v.loadObjectFunc);
            }
        }
        public int StartLoad()
        {
            int count = 0;
            if (this.mDependBundles != null)
            {
                foreach (var v in mDependBundles)
                {
                    if (!AsyncResourceManager.CheckLoaded(v.mPath))
                    {
                        count += v.StartLoad();
                    }
                    else
                    {
                        count += 1;
                    }
                }
            }
            if (!AsyncResourceManager.CheckLoaded(mMainBundle.mPath))
            {
                count += mMainBundle.StartLoad();
                this.mLoadCount = count;
            }
            else
            {
                count = count + 1;
                this.mLoadCount = count;
            }
            return count;
        }
        public bool CheckLLoad()
        {
            if (!mMainBundle.CheckLoad() && !AsyncResourceManager.CheckLoaded(mMainBundle.mPath))
            {
                return false;
            }
            if (this.mDependBundles != null)
            {
                foreach (var v in this.mDependBundles)
                {
                    if (!v.CheckLoad() && !AsyncResourceManager.CheckLoaded(v.mPath))
                    {
                        return false;
                    }
                }
            }

            return mMainBundle.CheckBundlePreloadAsset();
        }
    }
	
}
