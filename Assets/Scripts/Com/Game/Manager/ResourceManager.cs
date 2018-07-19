using System.Collections.Generic;
using UnityEngine;
using Com.Game.Core;
using System;
using Assets.Scripts.Com.Game.Utils;
using Assets.Scripts.Com.Game.Events;
using Assets.Scripts.Com.Game;
using Assets.Scripts.Com.Game.Manager;
using UnityEngine.Networking;
using static Com.Game.Manager.SyncResourceManager;

namespace Com.Game.Manager
{
    public class ResourceManager : Singleton<ResourceManager>
    {
        static public void LoadPrefab(string viewPath, string viewName, CallBackDelegate<object, object> CallBack)
        {
#if UNITY_EDITOR_WIN || UNITY_EDITOR_OSX
            GameObject go = UnityEditor.AssetDatabase.LoadAssetAtPath("Assets/RawResources/" + viewPath + viewName + ".prefab", typeof(GameObject)) as GameObject;
            
            GameObject instance = GameObject.Instantiate(go) as GameObject;
            CallBack(instance, null);
           // luaCallBack.Call(instance);
#endif
        }

        static public void LoadAsset(string assetPath, string assetName, Type type, CallBackDelegate<object, object> CallBack)
        {
#if UNITY_EDITOR_WIN || UNITY_EDITOR_OSX
            string file = "Assets/RawResources/" + assetPath + assetName;
            UnityEngine.Object o = UnityEditor.AssetDatabase.LoadAssetAtPath(file, type);
            if (o == null)
            {
                Debug.LogError("------------------download error:"+ file);
            }
            CallBack(o, null);
          //  luaCallBack.Call(o);
#endif
        }

        static public void LuaSendEvent(int value)
        {
            switch (value)
            {
                case 0:
                    EventDispatcher.Instance.Dispatch(EventConstant.OPEN_LOGIN_VIEW);
                    return;
            }

            Debug.LogError("LuaSendEven error:" + value);
        }

       /* static public void GetPlatformConfig(CallBackDelegate<object, object> callBack)
        {
            if (Launch.sLastConfig != null)
            {
                callBack.Call(Launch.sLastConfig);
            }
            else
            {
                ResourceManager.Instance.PriorLoadPersistent(false, PlatformManager.GetPlatformConfigPath(), delegate(WWW www)
                {
                    callBack.Call(new IniFile(www.assetBundle.LoadAsset<TextAsset>("config").text));
                });
            }
        }*/

        public void Update()
        {
            int localCount = localLoadingList.Count;
            if (localCount > 0)
            {
                for (int i = localCount - 1; i >= 0; i--)
                {
                    var info = localLoadingList[i];
                    if (info.CheckLoad() && i == localLoadingList.Count - 1)
                    {
                        localLoadingList.RemoveAt(i);
                        info.Unload();
                    }
                }
            }

            int remoteCount = remoteLoadingList.Count;
            if (remoteCount > 0)
            {
                int count = 0;
                ulong loadBytes = 0;
                for (int i = remoteCount - 1; i >= 0; i--)
                {
                    var info = remoteLoadingList[i];
                    if (info.CheckLoad())
                    {
                        if (i == remoteLoadingList.Count - 1)
                        {
                            remoteLoadingList.RemoveAt(i);
                            info.DoCallBack();
                        }
                        else
                        {
                            loadBytes += info.downloadedBytes;
                        }
                    }
                    else
                    {
                        loadBytes += info.downloadedBytes;

                        if (++count > 3)
                        {
                            break;
                        }
                    }

                    if (info.errorStr != null)
                    {
                        LoadRemoteErrorHandle(info.errorStr);
                        return;
                    }
                }

              //  EventDispatcher.Instance.Dispatch<float, float>(EventConstant.UPDATE_PROGRESS, Launch.mLoadedFileSize + (float)loadBytes, Launch.mTotalFileSize);
            }
        }

        private List<LocalLoadInfo> localLoadingList = new List<LocalLoadInfo>();
        public void LoadLocal(string str, Action<WWW> callBack)
        {
            LocalLoadInfo info = new LocalLoadInfo();
            info.callBack = callBack;
            info.path = str;
            localLoadingList.Insert(0, info);
        }

        private List<RemoteLoadInfo> remoteLoadingList = new List<RemoteLoadInfo>();
        public void LoadRemote(string str, Action<RemoteLoadInfo> callBack, bool isAssetBundle)
        {
            RemoteLoadInfo info = new RemoteLoadInfo();
            info.callBack = callBack;
            info.path = str;
            info.isAssetBundle = isAssetBundle;
            remoteLoadingList.Insert(0, info);
        }

        private void LoadRemoteErrorHandle(string error)
        {
            int remoteCount = remoteLoadingList.Count;
            if (remoteCount > 0)
            {
                for (int i = remoteCount - 1; i >= 0; i--)
                {
                    var info = remoteLoadingList[i];
                    info.UnLoad();
                }
            }
            remoteLoadingList.Clear();

            EventDispatcher.Instance.Dispatch<string, Action>(EventConstant.SHOW_ALERT, error, delegate()
            {
                EventDispatcher.Instance.Dispatch(EventConstant.RETRY_CHECK_VERSION);
            });
        }

        public void LoadPersistentAndRaw(string path, Action<WWW> persistentCallBack, Action<WWW> rawCallBack)
        {
            if (ResourceURL.CheckPersistentFileExists(path))
            {
                LoadLocal(ResourceURL.GetPersistentPath(path), persistentCallBack);
            }

            LoadLocal(ResourceURL.GetRawPath(path), rawCallBack);
        }

        public void PriorLoadPersistent(bool priorLoadPersistent, string path, Action<WWW> callBack)
        {
            if (priorLoadPersistent && ResourceURL.CheckPersistentFileExists(path))
            {
                LoadLocal(ResourceURL.GetPersistentPath(path), callBack);
            }
            else
            {
                LoadLocal(ResourceURL.GetRawPath(path), callBack);
            }
        }
    }


}