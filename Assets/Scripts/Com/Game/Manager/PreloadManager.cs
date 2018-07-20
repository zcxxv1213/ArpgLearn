using Com.Game.Core;
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
