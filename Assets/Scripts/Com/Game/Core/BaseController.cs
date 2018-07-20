using Assets.Scripts.Com.Game.Events;
using UnityEngine;

namespace Assets.Scripts.Com.Game.Core
{
    public class BaseController<T> : EventDispatcherInterfaceSingleton<T> where T : new()
    {
        private bool mIsInit;
        protected BaseController()
        {

        }

        /**
         * 初始化socket监听器和初始化逻辑间的消息侦听 
         */
        public void Init()
        {
            if (mIsInit == false)
            {
                mIsInit = true;
                AddNetListener();
                AddEventListeners();
                InitInChild();
            }
        }

        protected virtual void InitInChild()
        {

        }

        protected virtual void AddNetListener()
        {

        }

        protected virtual void AddEventListeners()
        {

        }
    }
}
