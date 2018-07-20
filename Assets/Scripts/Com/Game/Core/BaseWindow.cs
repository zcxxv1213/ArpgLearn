using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.Scripts.Com.Game.Enum;
using Assets.Scripts.Com.Game.Events;
using Assets.Scripts.Com.Game.Manager;
using Assets.Scripts.Com.Game.ConfigControl;
using UnityEngine;
using Com.Game.Utils.Timers;

namespace Assets.Scripts.Com.Game.Core
{
    public class BaseWindow : BaseView
    {
        public bool mDisposeRemoveViewID = true;
        public int mViewID;

        protected BackgroundImagesManager mBackgroundImagesManager = BackgroundImagesManager.Instance;

        protected static UIManager sUIManager = UIManager.Instance;

        private List<GameObject> mBackgroundImagesList = new List<GameObject>();

        //返回上一界面,check控制点击返回按钮关闭界面时触发检查是否需要直接关闭界面，比如关闭前需要弹窗确认;
        protected void ReturnUI(bool check = true)
        {
            Dispatch<bool>(EventConstant.RETURN_UI, check);
        }

        //打开界面
        protected void OpenUI(int viewEnum)
        {
            Dispatch<int>(EventConstant.OPEN_UI, viewEnum);
        }

        //点击返回按钮关闭界面时触发检查是否需要直接关闭界面，比如关闭前需要弹窗确认;
        public virtual bool OnReturnTriggerHideView()
        {
            return true;
        }

        //返回界面时候调用接口
        public virtual void OnReturnShowView()
        {

        }

        //点击返回按钮关闭界面，主要是为了HideRebuild逻辑区分是点击返回按钮关闭，还是打开其它界面关闭
        public virtual void OnReturnHideView()
        {

        }

        protected override void InternalInit()
        {
            GameObject layer = mViewParam.ParentLayer;
            if (layer == null || mViewParam.autoSortPanel != 0)//检测是否需要禁止panel的自动层级管理
            {
                base.InternalInit();
                return;
            }

            //设置所有子panel的深度在父panel上面
            
            base.InternalInit();
        }

        protected List<int> mPanelRenderQueueList = new List<int>();

        int parentLayerDepth;
        public int GetValidDepth()
        {
            return ++parentLayerDepth;
        }

        private List<BaseWindow> mUnderLayerWindowList;

        protected override void InternalOnViewShow()
        {
            base.InternalOnViewShow();

            sUIManager.OnLayerShow(mViewParam.ParentLayer, this);

            if (CheckHideUnderLayer())
            {
                mUnderLayerWindowList = sUIManager.GetUnderNormalLayerWindows(mViewParam.ParentLayer);
                for (int i = 0, count = mUnderLayerWindowList.Count; i < count; i++)
                {
                    BaseWindow baseWindow = mUnderLayerWindowList[i];

                    baseWindow.CheckUnderLayerWindowHide(this);
                }
            }
        }

        protected override void InternalOnViewHide()
        {
            base.InternalOnViewHide();

            sUIManager.OnLayerHide(mViewParam.ParentLayer, this);

            if (CheckHideUnderLayer())
            {
                if (mUnderLayerWindowList == null)
                    return;

                for (int i = 0, count = mUnderLayerWindowList.Count; i < count; i++)
                {
                    BaseWindow baseWindow = mUnderLayerWindowList[i];

                    baseWindow.CheckUnderLayerWindowShow(this);
                }

                mUnderLayerWindowList.Clear();
            }
        }

        private BaseView mOverWindow;
        private void CheckUnderLayerWindowHide(BaseWindow baseWindow)
        {
            if (gameObject != null && gameObject.activeSelf && mIsShow)
            {
                mOverWindow = baseWindow;
                gameObject.SetActive(false);
            }
        }

        private void CheckUnderLayerWindowShow(BaseWindow baseWindow)
        {
            if (gameObject != null && gameObject.activeSelf == false && mIsShow && mOverWindow == baseWindow)
            {
                mOverWindow = null;
                BeforeOnUnderLayerWindowShow();
                gameObject.SetActive(true);
                OnUnderLayerWindowShow();
            }
        }

        bool CheckHideUnderLayer()
        {
            return mViewParam.hideUnderLayer || string.IsNullOrEmpty(mViewParam.bannerName) == false;
        }

        protected virtual void BeforeOnUnderLayerWindowShow()
        {

        }

        protected virtual void OnUnderLayerWindowShow()
        {

        }
        

        protected void AddBackgroundImages(GameObject go)
        {
            mBackgroundImagesList.Add(go);
        }

        protected GameObject SetBackground(BackgroundType type)
        {
            GameObject go = mBackgroundImagesManager.SetBackground(gameObject, type);
            AddBackgroundImages(go);
            return go;
        }

        protected override void OnViewShow()
        {
            base.OnViewShow();

            OnPlayOpenSound();
            
        }
        GameTimer mHideAllActorsTimer;

        protected override void OnViewHide()
        {
            base.OnViewHide();

            OnPlayCloseSound();
            

            if (mHideAllActorsTimer != null)
            {
                mHideAllActorsTimer.Dispose();
            }
        }

        protected virtual void OnPlayOpenSound()
        {
           
        }
        protected virtual void OnPlayCloseSound()
        {
           
        }

        public virtual object GetRebuildParam()
        {
            return null;
        }
    }
}
