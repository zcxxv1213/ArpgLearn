using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Com.Manager.AssetLoader
{
    public class ABInfo
    {
        private int refCount;
        public string Name;
        private List<GameObject> refObj = new List<GameObject>();
        private bool ifTexture = false;
        public int RefCount
        {
            get
            {
                return this.refCount;
            }
            set
            {
                this.refCount = value;
            }
        }
        public bool IfTexture
        {
            get
            {
                return this.ifTexture;
            }
            set
            {
                this.ifTexture = value;
            }
        }
        public AssetBundle AssetBundle;

        public ABInfo(string name, AssetBundle ab)
        {
            this.Name = name;
            this.AssetBundle = ab;
            this.RefCount = 0;
        }
        public void AddRefObj(GameObject obj)
        {
            this.refCount++;
            refObj.Add(obj);
        }
        public bool CheckRefObj()
        {
            for (int i = refObj.Count - 1; i >= 0; i--)
            {
                if (refObj[i].Equals(null))
                {
                    this.refCount--;
                    refObj.Remove(refObj[i]);
                }
            }

            if (this.refCount > 0)
            {
                return false;
            }
            else
            {
                return true;
            }
        }
        public void Dispose()
        {
            if (this.AssetBundle != null)
            {
                this.AssetBundle.Unload(true);
            }
        }
    }
}
