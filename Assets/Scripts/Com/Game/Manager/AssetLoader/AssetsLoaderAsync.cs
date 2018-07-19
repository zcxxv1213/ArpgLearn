using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
namespace Com.Manager.AssetLoader
{
    public class AssetsLoaderAsync
    {

        private AssetBundle assetBundle;

        private AssetBundleRequest request;

        private TaskCompletionSource<bool> tcs;
        private string mAssetBundleName;
        public bool tag = false;
        public void SetBundle(AssetBundle ab, string assetBundleName)
        {
            mAssetBundleName = assetBundleName;
            this.assetBundle = ab;
        }
        public string ReturnAssetBundleName()
        {
            return mAssetBundleName;
        }
        public void Update()
        {
            if (!tag)
            {
                if (!this.request.isDone)
                {
                    return;
                }
                TaskCompletionSource<bool> t = tcs;
                t.SetResult(true);
                tag = true;
            }
        }
        public void Dispose()
        {
            this.assetBundle = null;
            this.request = null;
            this.tcs = null;
        }
        public async Task<UnityEngine.Object[]> LoadAllAssetsAsync()
        {
            await InnerLoadAllAssetsAsync();
            return this.request.allAssets;
        }

        public async Task<UnityEngine.Object> LoadAssetAsync(string assetName)
        {
            await InnerLoadAssetAsync(assetName);
            return this.request.asset;
        }

        private Task<bool> InnerLoadAssetAsync(string assetName)
        {
            this.tcs = new TaskCompletionSource<bool>();
            this.request = this.assetBundle.LoadAssetAsync(assetName);
            return this.tcs.Task;
        }

        private Task<bool> InnerLoadAllAssetsAsync()
        {
            this.tcs = new TaskCompletionSource<bool>();
            this.request = this.assetBundle.LoadAllAssetsAsync();
            return this.tcs.Task;
        }
    }
}
