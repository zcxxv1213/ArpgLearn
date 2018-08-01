using Assets.Scripts.Com.Game.Enum;
using Assets.Scripts.Com.Game.Events;
using Com.Game.Core;
using Com.Game.Manager;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts.Com.Game.Manager
{
    class PreloadManager : Singleton<PreloadManager>
    {
        public void PreloadBattleViewEffect()
        {
            List<string> list = new List<string>();


        }

        public void OnLoginEnterMainScenePreload(Action callBack)
        {
            UIManager.Instance.PreloadUI(ViewEnum.MainInterfaceView);
            SyncResourceManager.SetLoadAllFinishCallBack((object T1, object T2) =>
            {
                callBack();
                EventDispatcher.Instance.Dispatch<int, ToggleUIType, object>(EventConstant.TOGGLE_UI_WITH_PARAM, ViewEnum.LoginView, ToggleUIType.toggleFalse,
                    new object[] { //ArenaModel.Instance.mSelfInfo, ArenaModel.Instance.mEnemyInfo, ViewEnum.LoginView

                    });
            }
                );
        }

        private List<string> mEffectPreloadList;
        private List<int> mSoundPreloadList;

        private bool mDontDestoryAssetUnit = false;

        public void Init()
        {
            if (mEffectPreloadList == null)
                mEffectPreloadList = new List<string>();
            mEffectPreloadList.Clear();

            if (mSoundPreloadList == null)
                mSoundPreloadList = new List<int>();
            mSoundPreloadList.Clear();

            mDontDestoryAssetUnit = false;
        }
    }
}
