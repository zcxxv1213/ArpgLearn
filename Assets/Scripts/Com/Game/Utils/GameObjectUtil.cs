using Assets.Scripts.Com.Game.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Scripts.Com.Game.Utils
{
    public static class GameObjectUtil
    {
        public static UnityEngine.Object Instantiate(UnityEngine.Object original)
        {
            return GameObject.Instantiate(original);
        }

        public static void SetParent(Transform childTrans, Transform parentTrans)
        {
            childTrans.SetParent(parentTrans);
            childTrans.localPosition = Vector3.zero;
            childTrans.localScale = Vector3.one; 
        }

        public static void SetLayer(GameObject go, int layer, bool loopChild = false)
        {
            go.layer = layer;
            if (loopChild)
            {
                Transform t = go.transform;
                for (int i = 0, imax = t.childCount; i < imax; ++i)
                {
                    Transform child = t.GetChild(i);
                    SetLayer(child.gameObject, layer, loopChild);
                }
            }
        }

        public static void SetTag(GameObject go, string tag, bool loopChild = false)
        {
            go.tag = tag;
            if (loopChild)
            {
                Transform t = go.transform;
                for (int i = 0, imax = t.childCount; i < imax; ++i)
                {
                    Transform child = t.GetChild(i);
                    SetTag(child.gameObject, tag, loopChild);
                }
            }
        }

        public static void Reset(GameObject go, bool isWorldChange = false)
        {
            Transform t = go.transform;
            if (isWorldChange)
            {
                t.position = Vector3.zero;
                t.rotation = Quaternion.identity;
                t.localScale = Vector3.one;
            }
            else
            {
                t.localPosition = Vector3.zero;
                t.localRotation = Quaternion.identity;
                t.localScale = Vector3.one;
            }
        }

        public static void AddChild(GameObject parent, GameObject child)
        {
            Transform transform = child.transform;

            transform.parent = parent.transform;
            transform.localScale = parent.transform.localScale;
            transform.localPosition = Vector3.zero;
        }
     
    }
}
