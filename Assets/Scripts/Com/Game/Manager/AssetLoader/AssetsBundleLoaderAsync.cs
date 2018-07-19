using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
namespace Com.Manager.AssetLoader {
    public class AssetsBundleLoaderAsync 
    {
        private AssetBundleCreateRequest creatRequest;

        private TaskCompletionSource<AssetBundle> tcc;
        private string assetBundleName;
        public bool tag = false;
        public void Update()
        {
            if (!tag)
            {

                if (!this.creatRequest.isDone)
                {
                    return;
                }

                TaskCompletionSource<AssetBundle> t = tcc;
                t.SetResult(this.creatRequest.assetBundle);
                tag = true;
            }
        }
        public string ReturnBundleName()
        {
            return assetBundleName;
        }
        public void Dispose()
        {
            this.creatRequest = null;
            this.tcc = null;
        }
        public Task<AssetBundle> LoadAsync(string bundleName, string path)
        {
            assetBundleName = bundleName;
            this.tcc = new TaskCompletionSource<AssetBundle>();
            this.creatRequest = AssetBundle.LoadFromFileAsync(Application.streamingAssetsPath + "/" + bundleName);
            return this.tcc.Task;
        }
    }
}
