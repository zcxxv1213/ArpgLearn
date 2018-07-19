using Com.Game.Manager;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using static Com.Game.Manager.SyncResourceManager;

namespace Com.Manager.AssetLoader
{
    public class MainBundle :Bundle
    {
        private const int mAudioClipType = 0;
        private const int mUIType = 1;
        private const int mUIFxType = 2;
        private const int mUITexture = 3;
        private const int mExternalGo = 4;
        private const int mExternalTexture = 5;
        private const int mExternalRole = 6;

        public Dictionary<string,Object> mAssets = new Dictionary<string, Object>();
        public Dictionary<int, Object> mInstanceReferences = new Dictionary<int, Object>();
        
        public AssetBundleRequest mLoadAssetAsync;
        public Bundle[] mDependBundles;
        public int? mAssetType;
        static Regex uiLoginRex = new Regex("ui_login");
        public MainBundle(string path):base(path)
        {

        }

        public bool CheckBundlePreloadAsset()
        {
            if (this.mExternalScene)
            {
                return this.CheckScenePreloadAsset();
            }
            else
            {
                return this.CheckResoucePreloadAsset();
            }
        }

        private bool CheckResoucePreloadAsset()
        {
            if (this.mBundle != null)
            {
                if (mPreloadAssetName != null)
                {
                    if (this.mLoadAssetAsync != null)
                    {
                        if (this.mLoadAssetAsync.isDone)
                        {
                            this.mAssets[this.mPreloadAssetName] = this.mLoadAssetAsync.asset;
                            return true;
                        }
                    }
                    return true;
                }
                else
                    return true;
            }
            return false;
        }

        private bool CheckScenePreloadAsset()
        {
            if (this.mBundle != null)
            {
                return true;
            }
            return false;
        }
        public override void SetDependBundles(Bundle[] dependBundles)
        {
            if (mDependBundles==null)
            {
                mDependBundles = dependBundles;
                this.AddDependBundlesRef();
            }
        }
        private void AddDependBundlesRef()
        {
            if (mDependBundles == null)
                return;
            foreach (var v in mDependBundles)
            {
                v.AddRef(this);
            }
        }
        public void ReduceDependBundlesRef()
        {
            if (mDependBundles == null)
                return;
            foreach (var v in mDependBundles)
            {
                v.ReduceRef(this);
            }
        }
        public override void LoadParams(string objectName, CallBackDelegate<object, object> callBack, LoadFunc loadObjectFunc = null )
        {
            loadObjectFunc?.Invoke(this, objectName, callBack);
        }
        public override void LoadObject(string assetName, CallBackDelegate<object, object> callBack,bool instantiate,int assetType)
        {
            this.mAssetType = assetType;
            if (!this.mAssets.ContainsKey(assetName))
            {
                if (this.mBundle==null)
                {
                    Debug.Log("Bundle Null" + assetName);
                }
                this.mAssets[assetName] = this.mBundle.LoadAsset(assetName);
            }
            if (instantiate == true)
            {
                Object assetInstance = Object.Instantiate(this.mAssets[assetName]);
                this.mInstanceReferences[this.mInstanceReferences.Count + 1] = assetInstance;
                // callBack.Call(assetInstance);
                callBack(assetInstance,null);
            }
            else
            {
                Debug.Log("NoInstance");
                callBack(this.mAssets[assetName],null);
               // callBack.Call(this.mAssets[assetName]);
            }
        }
        public override void PreLoadFunc()
        {
            this.mAssetType = null;
        }
        public override void CheckUnload()
        {
            if (this.mAssetType == null)
            {
                return;
            }
            bool beCheck = false;
            Match isLogin = uiLoginRex.Match(this.mPath);
            if (this.mAssetType == mUIFxType && isLogin.Success)
            {
                beCheck = true;
            }
            if (this.mAssetType == mUIType || this.mAssetType == mExternalGo || beCheck == true)
            {
                int count = this.mInstanceReferences.Count;
                if (count > 0)
                {
                    for (int i = count; i == 1; i--)
                    {
                        if (this.mInstanceReferences[i].Equals(null))
                        {
                            this.mInstanceReferences.Remove(i);
                            //count -= 1; 
                        }
                    }
                }
                if (this.mInstanceReferences.Count <= 0)
                {
                    this.Dispose(true);
                }
            }
            else if (this.mAssetType == mAudioClipType)
            {
                this.Dispose(false);
            }
            else if (this.mAssetType == mUITexture)
            {
                this.Dispose(false);
            }
        }
        public override void Dispose(bool unloadAllLoadedObjects)
        {
            base.Dispose(unloadAllLoadedObjects);
            this.mAssets.Clear();
            this.ReduceDependBundlesRef(); ;
            this.mDependBundles = null;
            this.mAssetType = null;
        }
    }
}
