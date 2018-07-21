using UnityEngine;
using Assets.Scripts.Com.Game.Events;
using Assets.Scripts.Com.Game.Enum;
using System;
using System.Collections.Generic;
using Assets.Scripts.Com.Game.Manager;
using Assets.Scripts.Com.Game.Utils;
using Com.Game.Utils.Timers;

namespace Assets.Scripts.Com.Game.Core
{
    public class BaseViewParam
    {
        public string url = ""; //资源包路径路径
        public string viewName = "";//资源名称
        public GameObject ParentLayer = null;
        public BgEnum bgEnum = BgEnum.NONE;
        public string bannerName = "";
        public int autoSortPanel = 0; //不等于0的话是禁止自动排序
        public bool showUILoading = false;
        public bool syncLoad = false;
        public bool usePool = false;
        public bool forbidSound = false; //默认不屏蔽音效
        public bool hideUnderLayer = false;//显示时隐藏低层级的所有窗口，隐藏时显示低层级的所有窗口
        public bool forbidReplacePrevView = false;
        public bool openInShortTime = false;
        public bool hideAllActors = false;
    }

    public class BaseView : EventDispatcherInterface, ITick
    {
        protected static readonly UIManager sUIManager = UIManager.Instance;

        protected string url { get { return mViewParam.url; } } //资源包路径路径
        protected string viewName { get { return mViewParam.viewName; } }//资源名称

        public bool mIsShow { get; private set; }
        public bool mIsLoadComplete { get; private set; }

        public int mLastShowFrameCount;

        public Transform transform { get; private set; }
        public GameObject gameObject { get; private set; }

        private int mLoadedFrameCount;

        protected GameObject mBackgroundGameObject;

        public event Action mLoadCompleteHandler;
        public event Action mShowHandler;

        public object mParam { get; protected set; }

        private bool mIsDestory;

        protected const int cEffectDelayLoad = 15;

        public GameObject GetParentLayer()
        {
            return mViewParam.ParentLayer;
        }

        public BaseViewParam mViewParam { get; protected set; }

        public BaseView()
        {
            mIsShow = false;

            mViewParam = InitViewParam();
        }

        public BaseView(GameObject go)
            : this()
        {
            InjectGameObject(go);
        }

        protected virtual BaseViewParam InitViewParam()
        {
            return new BaseViewParam();
        }

        //path只支持完整路径，不支持名字查找
        public T FindComponent<T>(string path) where T : Component
        {
            GameObject obj = FindChild(path);

            if (obj == null)
                return null;

            return obj.GetComponent<T>();
        }

        public GameObject FindChild(string path)
        {
            Transform t = transform.Find(path);
            if (t == null)
                return null;

            GameObject go = t.gameObject;
            return go;
        }

        public GameObject FindChild(Transform transform, string path)
        {
            return transform.Find(path).gameObject;
        }

        public void SetParent(GameObject parentObj)
        {
            if (parentObj == null)
                return;

            GameObjectUtil.AddChild(parentObj, gameObject);
        }

        //控制全局鼠标点击
        public void GlobalClickEnable(bool enable)
        {
            sUIManager.GlobalClickEnable(enable);
        }

        //底层调用，逻辑层不能够直接重载
        protected virtual void OnBeforeLoadView()
        {

        }

        private bool ShowUILoading(bool isShow)
        {
            if (mPreloadShowView == false && mViewParam.showUILoading)
            {
                if (isShow)
                {
                 //   Dispatch<bool>(EventConstant.SHOW_UI_LOADING, isShow);
                }
                else
                {
                   
                }
                return true;
            }

            return false;
        }

        //加载资源
        private void LoadView()
        {
            //如果没有外部传入的界面参数或者提前构造了导致ParentLayer为null，则需要初始化界面参数
            if (mViewParam == null || mViewParam.ParentLayer == null)
                mViewParam = InitViewParam();

            if (string.IsNullOrEmpty(url))
            {
                Debug.LogError("BaseView url error");
                return;
            }

            OnBeforeLoadView();

            //if (Application.isEditor == false)
            mViewParam.showUILoading = false;

            ShowUILoading(true);

            mIsDestory = false;
            sUIManager.LoadView(url, viewName, OnLoadCallBack);
        }

        private void OnLoadCallBack(GameObject gameObject)
        {
            InjectGameObjectInternal(gameObject);

            if (mLoadCompleteHandler != null)
            {
                mLoadCompleteHandler();
                mLoadCompleteHandler = null;
            }

            if (mPreloadShowView)
            {
                mPreloadShowView = false;
                mIsShow = false;
                gameObject.SetActive(false);
                return;
            }

            if (ShowUILoading(false))
                return;

            if (mIsShow)
            {
                OnShow(true);
            }
            else
            {
                OnHide();
            }
        }

        private static List<BaseView> sWindowList = new List<BaseView>();
        public static void DebugActiveWindows()
        {
            if (Application.isEditor == false)
                return;

            Debug.LogWarning("start---输出未调用CloseWindow的BaseView类实例,仅供程序参考---");

            for (int i = 0, count = sWindowList.Count; i < count; i++)
            {
                BaseView window = sWindowList[i];

                if (window.IsDestory() == false)
                {
                    Debug.LogWarning(string.Format("index:{0},activeWindow:{1}", i, window));
                }
            }

            sWindowList.Clear();

            Debug.LogWarning("end---输出未调用CloseWindow的BaseView类实例,仅供程序参考---");
        }

        //加载完成传入游戏对象
        private void InjectGameObjectInternal(GameObject gameObject)
        {
            if (Application.isEditor)
            {
                sWindowList.Add(this);
            }

            this.gameObject = gameObject;
            transform = gameObject.transform;
            mIsLoadComplete = true;
            mLoadedFrameCount = Time.frameCount;

            GameObject layer = mViewParam.ParentLayer;
            SetParent(layer);

            InternalInit();
        }

        //从外部传入游戏对象
        public void InjectGameObject(GameObject gameObject)
        {
            this.gameObject = gameObject;
            transform = gameObject.transform;
            mIsLoadComplete = true;
            mIsShow = gameObject.activeSelf;

            InternalInit();
        }

        public void ToggleShow(bool isShow)
        {
            if (isShow)
                ShowView();
            else
                HideView();
        }

        public void ToggleShow()
        {
            ToggleShow(!mIsShow);
        }

        protected bool mPreloadShowView { get; private set; }
        //预加载界面
        public void PreloadView()
        {
            mPreloadShowView = true;

            if (gameObject == null)
            {
                LoadView();
            }
        }

        public void ShowView(object param)
        {
            mParam = param;

            ShowView();
        }

        public void ShowView(Action callBack, BaseViewParam baseViewParam = null)
        {
            if (mIsLoadComplete == false)
            {
                if (baseViewParam != null)
                    mViewParam = baseViewParam;

                if (callBack != null)
                    mLoadCompleteHandler += callBack;
            }

            ShowView();
        }

        public void ShowView()
        {
            if (mIsShow == true)
            {
                OnRepeatShow();
                return;
            }

            mIsShow = true;

            OnShow();
        }

        protected virtual void OnRepeatShow()
        {

        }

        bool mLoad = false;
        private void OnShow(bool afterLoadComplete = false)
        {
            if (gameObject == null)
            {
                LoadView();
                mLoad = true;
            }
            else
            {
                if (gameObject != null)
                {
                    gameObject.SetActive(mIsShow);
                    InternalOnViewShow();
                    
                }
            }
        }

        private bool mPlayingEffect = false;
        private void ShowEffect()
        {
            if (mIsShow == false)
                return;

            //Application.targetFrameRate = -1;
            gameObject.SetActive(mIsShow);

            if (mBackgroundGameObject != null)
            {
                mBackgroundGameObject.transform.parent = transform.parent;
            }

           /* if (mViewParam.ParentLayer != null)
                mParentPanel = mViewParam.ParentLayer.GetComponent<UIPanel>();

            TickManager.AddTick(this);
            mPlayingEffect = true;
            OnBeginEffect();

            ViewAnimationManager.Instance.Show(mViewParam.viewEffectEnum, gameObject, AnimationCompleteCallBack);*/
        }

        private void AnimationCompleteCallBack()
        {
            if (mPlayingEffect == false)
                return;

            if (mBackgroundGameObject != null)
            {
                mBackgroundGameObject.transform.parent = transform;
            }

            OnTick();
            TickManager.RemoveTick(this);
            mPlayingEffect = false;
            OnEndEffect();
            //Main.SetTargetFrameRate();
        }

        protected virtual void OnBeginEffect()
        {

        }

        protected virtual void OnEndEffect()
        {

        }

        protected virtual void OnEffectPlaying()
        {

        }

        int mTickIndex = 0;
        public void OnTick()
        {
            if (++mTickIndex % 3 == 0)
            {
               
            }
        }

        public void OnChangeScene()
        {

        }

        public void HideView()
        {
            if (mIsShow == false)
                return;

            mIsShow = false;

            OnHide();
        }

        private void OnHide()
        {
            if (gameObject != null)
            {
                InternalOnViewHide();
                gameObject.SetActive(mIsShow);
            }
        }

        //销毁界面
        public void CloseView()
        {
            HideView();

            if (gameObject != null)
            {
                InternalDispose();

                GameObject.DestroyImmediate(gameObject);

                this.transform = null;
                this.gameObject = null;
                this.mBackgroundGameObject = null;
                this.mIsLoadComplete = false;
            }

            InternalToggleListener(false, true);

            mIsDestory = true;
        }

        //底层调用，逻辑层不能够直接重载
        protected virtual void InternalDispose()
        {
            Dispose();
        }

        protected virtual void Dispose()
        {

        }

        public bool IsDestory()
        {
            return mIsDestory;
        }

        //底层调用，逻辑层不能够直接重载
        protected virtual void InternalInit()
        {
            AddBackground(mViewParam.bgEnum);

            Init();
        }
        //TODO ChangeBackGroundType
        protected void AddBackground(BgEnum bgEnum)
        {
            switch (bgEnum)
            {
                case BgEnum.GRAY:
                    mBackgroundGameObject = BackgroundImagesManager.Instance.SetBackground(gameObject, BackgroundType.BG_3);
                    //TODO AddBtbListener
                  //  GameUIEventListener.Get(mBackgroundGameObject, 0).onClick = OnClickHideView;
                    break;
                case BgEnum.TRANSPARENT:
                    mBackgroundGameObject = BackgroundImagesManager.Instance.SetBackground(gameObject, BackgroundType.BG_3);
                    break;
                case BgEnum.TRANSPARENT_CLICKABLE:
                    mBackgroundGameObject = BackgroundImagesManager.Instance.SetBackground(gameObject, BackgroundType.BG_3);
                    //TODO AddBtbListener
                    // GameUIEventListener.Get(mBackgroundGameObject, 0).onClick = OnClickHideView;
                    break;
            }
        }

        protected virtual void OnClickHideView(GameObject go)
        {
            if (mPlayingEffect == false)
                HideView();
        }

        //初始化 控件  子类重写,只在加载完AssetBundle之后执行一次
        protected virtual void Init()
        {

        }

        private bool mSetViewShowSpeed;

        ////底层调用，逻辑层不能够直接重载
        protected virtual void InternalOnViewShow()
        {
            if (mShowHandler != null)
            {
                mShowHandler();
                mShowHandler = null;
            }

            mSetViewShowSpeed = mLoadedFrameCount > 0;
            if (mSetViewShowSpeed)
            {
                mLoadedFrameCount = 0;
            }

            OnViewShowWithParam(mParam);
            mParam = null;

            OnViewShow();

            InternalToggleListener(true);
        }

        ////底层调用，逻辑层不能够直接重载
        protected virtual void InternalOnViewHide()
        {
            AnimationCompleteCallBack();

            OnViewHide();

            InternalToggleListener(false);
        }

        //显示界面时接收外部传入的参数
        protected virtual void OnViewShowWithParam(object param)
        {

        }

        //打开UI后的处理 ，打开 View 的特效，动画 ,协议请求等处理 ，子类重写，基类调用
        protected virtual void OnViewShow()
        {
        }
        //关闭UI前的处理，关闭 View 的特效 ，动画 等处理 ，子类重写，基类调用,每次CloseView都会被调用
        protected virtual void OnViewHide()
        {
        }

        public void AddUIEffect(int delayFrameShow, RenderQueueEnum effectLayerEnum, GameObject effectParent, string effectName, Action<GameObject> callBack = null, bool resetScale = true, bool layerTop = true, bool dontDestoryAssetUnit = false)
        {
            GameTimer.ExecuteTotalFrames(delayFrameShow, null, delegate()
            {
                if (mIsDestory)
                    return;

                UIEffectManager.Instance.AddUIEffect(effectLayerEnum, effectParent, effectName, callBack, resetScale, layerTop, dontDestoryAssetUnit);
            }, true);
        }

        public void AddUIEffect(RenderQueueEnum effectLayerEnum, GameObject effectParent, string effectName, Action<GameObject> callBack = null, bool resetScale = true, bool layerTop = true, bool dontDestoryAssetUnit = false)
        {
            UIEffectManager.Instance.AddUIEffect(effectLayerEnum, effectParent, effectName, callBack, resetScale, layerTop, dontDestoryAssetUnit);
        }

        public void AddUIEffect(int delayFrameShow, int effectLayerEnum, GameObject effectParent, string effectName, Action<GameObject> callBack = null, bool resetScale = true, bool layerTop = true, bool dontDestoryAssetUnit = false)
        {
            GameTimer.ExecuteTotalFrames(delayFrameShow, null, delegate()
            {
                if (mIsDestory)
                    return;

                UIEffectManager.Instance.AddUIEffect(effectLayerEnum, effectParent, effectName, callBack, resetScale, layerTop, dontDestoryAssetUnit);
            }, true);
        }

        public void AddUIEffect(int effectLayerEnum, GameObject effectParent, string effectName, Action<GameObject> callBack = null, bool resetScale = true, bool layerTop = true, bool dontDestoryAssetUnit = false)
        {
            UIEffectManager.Instance.AddUIEffect(effectLayerEnum, effectParent, effectName, callBack, resetScale, layerTop, dontDestoryAssetUnit);
        }

        private Dictionary<EventConstant, List<KeyValuePair<Delegate, bool>>> mEventListeners;

        //toggle true:界面显示时会自动添加侦听，隐藏时会自动删除侦听 false:界面初始化时自动添加侦听，释放时才删除侦听
        protected void RegisterEventListener(EventConstant typeCode, EventDispatcher.EventCallback listener, bool toggle)
        {
            InternalRegisterEventListener(typeCode, listener, toggle);
        }

        //toggle true:界面显示时会自动添加侦听，隐藏时会自动删除侦听 false:界面初始化时自动添加侦听，释放时才删除侦听
        protected void RegisterEventListener<T1>(EventConstant typeCode, EventDispatcher.EventCallback<T1> listener, bool toggle)
        {
            InternalRegisterEventListener(typeCode, listener, toggle);
        }

        //toggle true:界面显示时会自动添加侦听，隐藏时会自动删除侦听 false:界面初始化时自动添加侦听，释放时才删除侦听
        protected void RegisterEventListener<T1, T2>(EventConstant typeCode, EventDispatcher.EventCallback<T1, T2> listener, bool toggle)
        {
            InternalRegisterEventListener(typeCode, listener, toggle);
        }

        //toggle true:界面显示时会自动添加侦听，隐藏时会自动删除侦听 false:界面初始化时自动添加侦听，释放时才删除侦听
        protected void RegisterEventListener<T1, T2, T3>(EventConstant typeCode, EventDispatcher.EventCallback<T1, T2, T3> listener, bool toggle)
        {
            InternalRegisterEventListener(typeCode, listener, toggle);
        }

        private void InternalRegisterEventListener(EventConstant typeCode, Delegate listener, bool toggle)
        {
            if (mEventListeners == null)
            {
                mEventListeners = new Dictionary<EventConstant, List<KeyValuePair<Delegate, bool>>>();
            }

            List<KeyValuePair<Delegate, bool>> list = null;
            if (mEventListeners.TryGetValue(typeCode, out list) == false)
            {
                list = new List<KeyValuePair<Delegate, bool>>();
                mEventListeners[typeCode] = list;
            }

            list.Add(new KeyValuePair<Delegate, bool>(listener, toggle));

            if (toggle == false)
                dispatcher.RegisterEventListener(typeCode, listener);
        }

        private void InternalToggleListener(bool show, bool force = false)
        {
            if (mEventListeners == null)
                return;

            foreach (KeyValuePair<EventConstant, List<KeyValuePair<Delegate, bool>>> kvp in mEventListeners)
            {
                List<KeyValuePair<Delegate, bool>> list = kvp.Value;

                for (int i = 0, count = list.Count; i < count; i++)
                {
                    KeyValuePair<Delegate, bool> value = list[i];

                    if (value.Value || force)
                    {
                        if (show)
                        {
                            dispatcher.RegisterEventListener(kvp.Key, value.Key);
                        }
                        else
                        {
                            dispatcher.DeleteEventListener(kvp.Key, value.Key);
                        }
                    }
                }
            }

            if (force)
            {
                mEventListeners.Clear();
            }
        }

    }
}

