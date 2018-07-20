using Assets.Scripts.Com.Game.Core;
using Assets.Scripts.Com.Game.Utils;
using Com.Game.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Scripts.Com.Game.Manager
{
    public enum BackgroundType
    {
        BG_1 = 1,
        BG_3 = 3,
    }

    public class BackgroundImagesManager : Singleton<BackgroundImagesManager>
    {
        public const string BG = "bg";
        const string bgPath = "UI/CommonBg/{0}.bytes";
        public GameObject SetBackground(GameObject gameObject, BackgroundType type)
        {
            string name = BG + (int)type;
            GameObject child = null;
            //TODO SyncLoaderMangerLoadAsset
           /* child = AssetLoader.Instance.LoadUIGameObject(string.Format(bgPath, name), name);
            child.transform.parent = gameObject.transform;
            child.transform.localScale = gameObject.transform.localScale;
            child.transform.localPosition = Vector3.zero;

            UIWidget widget = child.GetComponent<UIWidget>();
            if (widget != null)
                widget.depth = -1;
                */
            return child;
        }
    }
}
