using Assets.Scripts.Com.Game.Manager;
using Com.Game.Core;
using Com.Game.Manager;
using Com.Game.Utils.Timers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Assets.Scripts.Com.Manager
{
    public class MemoryManager : Singleton<MemoryManager>
    {
        public void BeforeEnterScene(bool reLogin,bool enterBattle,bool leaveBattle)
        {

        }
        public void ChangeScene(bool reLogin)
        {
            GameTimerManager.Instance.ClearOnChangeScene();
            UIManager.Instance.DisposeAllLayers(reLogin);
            //TODO PoolManagerDispose;
            AsyncResourceManager.UnloadBundles();
            SyncResourceManager.UnloadBundles();
            Resources.UnloadUnusedAssets();
        }
        public void EnterBattle()
        {
            GameTimerManager.Instance.ClearOnChangeScene();
            UIManager.Instance.DisposeAllLayers(false);
            //TODO PoolManagerDispose;
            AsyncResourceManager.UnloadBundles();
            SyncResourceManager.UnloadBundles();
            Resources.UnloadUnusedAssets();
        }
        public void LeaveBattle()
        {
            //TODO 战斗释放
        }
    }
}
