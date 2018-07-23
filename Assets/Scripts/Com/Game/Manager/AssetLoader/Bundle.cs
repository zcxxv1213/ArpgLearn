using Assets.Scripts.Com.Game.Utils;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Com.Game.Manager.SyncResourceManager;

namespace Com.Manager.AssetLoader
{
    public class Bundle
    {
        public string mPath;
        public bool mLoadCompleted = false;
        public int mRefCount = 0;
        public AssetBundle mBundle;
        public string mPreloadAssetName;
        public bool mExternalScene = false;
        public bool mIfFront = false;
        public bool mIsEditor = Application.isEditor;
        public Dictionary<MainBundle, MainBundle> mHostBundles = new Dictionary<MainBundle, MainBundle>();
        private WWW www;
        public Bundle(string path)
        {
            mPath = path;
            mIfFront = mPath == "common/hkyt5"; 
        }
        public int StartLoad()
        {
            if (this.www == null)
            {
                Debug.Log(ResourceURL.GetUrl(this.mPath));
                this.www = new WWW(ResourceURL.GetUrl(this.mPath));
                return 1;
            }
            return 0;
        }

        public bool CheckLoad()
        {
            if (this.mLoadCompleted)
            {
                return true;
            }

            if (this.www != null && this.www.isDone)
            {
                if (this.www.error == "")
                {
                    this.mBundle = this.www.assetBundle;
                    Debug.Log("CheckLoad: " + mPath);
                }
                else
                {
                    if (mIsEditor)
                    {
                        Debug.LogError(this.mPath);
                        Debug.LogError(this.www.error); 
                    }
                }
                AssetLoaderManager.Instance.mLoadedBunders[this.mPath] = www.assetBundle;
                this.mLoadCompleted = true;
                this.www.Dispose();
                return true;
            }
            return false;
        }
        public virtual void LoadParams(string objectName, CallBackDelegate<object, object> callBack, LoadFunc loadObjectFunc = null)
        {

        }
        public virtual void SetDependBundles(Bundle[] dependBundles)
        {

        }
        public virtual void PreLoadFunc()
        {

        }
        public virtual void LoadObject(string assetName, CallBackDelegate<object, object> callBack, bool instantiate, int assetType)
        {

        }
        public void AddRef(MainBundle hostBundle)
        {
            this.mRefCount += 1;
            this.mHostBundles[hostBundle] = hostBundle;
        }
        public void ReduceRef(MainBundle hostBundle)
        {
            this.mRefCount -= 1;
            this.mHostBundles.Remove(hostBundle); 
        }
        public virtual void CheckUnload()
        {
            if (this.mBundle==null)
            {
                return;
            }
            if (this.mRefCount == 0 && !this.mIfFront)
            {
                this.Dispose(true);
            }
        }
        public virtual void Dispose(bool unloadAllLoadedObjects)
        {
            if (this.mBundle==null)
            {
                return;
            }
            Debug.Log("Dispose: " + mPath );
            this.mBundle.Unload(unloadAllLoadedObjects);
            this.mLoadCompleted = false;
            this.mBundle = null;
            this.mRefCount = 0;
            this.www = null; 

        }
    }
}
