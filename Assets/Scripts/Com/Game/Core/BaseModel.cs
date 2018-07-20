using System.Collections.Generic;
using System;
using Assets.Scripts.Com.Game.Events;
using UnityEngine;

namespace Assets.Scripts.Com.Game.Core
{
    public class BaseModel<T> : EventDispatcherInterfaceSingleton<T> where T : new()
    {
        protected override void InternalInit()
        {
            //Debug.LogError("Init: T:" + typeof(T).Name);

            AddEventListener(EventConstant.CLEAR_BASE_MODEL, ClearBaseModel);
        }

        protected virtual void ClearBaseModel()
        {
            RemoveEventListener(EventConstant.CLEAR_BASE_MODEL, ClearBaseModel);

            Recreate();
        }
    }
}
