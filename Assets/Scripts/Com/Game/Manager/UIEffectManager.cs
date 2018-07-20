using Assets.Scripts.Com.Game.Core;
using Assets.Scripts.Com.Game.Utils;
using System;
using UnityEngine;
using Assets.Scripts.Com.Game.Enum;
using Com.Game.Core;

namespace Assets.Scripts.Com.Game.Manager
{
    public class UIEffectManager : Singleton<UIEffectManager>
    {
       
        public void AddUIEffect(int effectLayerEnum, GameObject effectParent, string effectName, Action<GameObject> callBack = null, bool resetScale = true, bool layerTop = true, bool dontDestoryAssetUnit = false,Action callBack2 = null)
        {
            //TODO LoadUI Effect SetLayerAndParent also Size
        }

        public void AddUIEffect(RenderQueueEnum effectLayerEnum, GameObject effectParent, string effectName, Action<GameObject> callBack = null, bool resetScale = true, bool layerTop = true, bool dontDestoryAssetUnit = false)
        {
            AddUIEffect((int)effectLayerEnum, effectParent, effectName, callBack, resetScale, layerTop, dontDestoryAssetUnit);
        }
    }
}
