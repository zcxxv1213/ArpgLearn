using Assets.Scripts.Com.Game.Core;
using UnityEngine;
using Assets.Scripts.Com.Game.Events;
using Assets.Scripts.Com.Game.Utils;
using System;
using Assets.Scripts.Com.Game.Enum;
using System.Collections.Generic;
using Com.Game.Core;
using Assets.Scripts.Com.Manager;
using Com.Game.Utils.Timers;

namespace Assets.Scripts.Com.Game.Manager
{
    public enum ToggleUIType
    {
        toggleTrue, //打开
        toggleFalse,//关闭
        toggleAuto //自动切换（界面开的话则关闭，界面关闭的话则打开）
    }

    public class UIManager : Singleton<UIManager>
    {

        public static Camera sUICamera;

        public Transform mTransform { get; private set; }

        GameObject mViewRoot;

        public int mDeviceWidth { get; private set; }
        public int mDeviceHeight { get; private set; }

        private GameObject mJoystick;
        private GameObject mLogin;
        public GameObject mNormal { get; private set; }
        private GameObject mBattle;
        private GameObject mGlobal;
        private GameObject mGlobalContainerPop3;
        private GameObject mGlobalContainerNet;
        public GameObject mLoginBackgroundLayer { get; private set; }
        public GameObject mLoginLayer { get; private set; }

        //战斗界面层级
        public GameObject mBattleBloodLayer { get; private set; }
        public GameObject mBattleLayer1 { get; private set; }
        public GameObject mBattleLayer2 { get; private set; }
        public GameObject mBattleLayer3 { get; private set; }
        public GameObject mBattlePop { get; private set; }

        //角色信息界面
        public GameObject mPlayerInfoView { get; private set; }

        //主界面
        public GameObject mMainInterface { get; private set; }

        //主要用于一级界面
        public GameObject mNormalLayer1 { get; private set; }

        //主要用于二级界面
        public GameObject mNormalLayer2 { get; private set; }
        //主要用于三级界面

        public GameObject mNormalLayer3 { get; private set; }

        public GameObject mNormalPop { get; private set; }

        public GameObject mNormalPop1 { get; private set; }

        public GameObject mNormalPop2 { get; private set; }

        public GameObject mNormalNotice { get; private set; }

        //全局界面的层级
        public GameObject mGlobalPop { get; private set; }
        public GameObject mGlobalPop2 { get; private set; }

        public GameObject mGlobalNet { get; private set; }
        public GameObject mSceneLoading { get; private set; }
        public GameObject mGameQuitView { get; private set; }

        private int mViewIndex = 0;
        private Dictionary<int, Type> mBaseWindowTypeDic = new Dictionary<int, Type>();
        private Dictionary<int, BaseWindow> mBaseWindowDic = new Dictionary<int, BaseWindow>();
        private Dictionary<int, string> mBaseWindowNameDic = new Dictionary<int, string>();
        private Dictionary<string, int> mBaseWindowIDDic = new Dictionary<string, int>();
        private Dictionary<GameObject, List<BaseWindow>> mBaseWindowParentDic = new Dictionary<GameObject, List<BaseWindow>>();

        private List<GameObject> mNormalLayerList = new List<GameObject>();
        private List<GameObject> mPopLayerList = new List<GameObject>();

        private EventDispatcher mEventDispatcher = EventDispatcher.Instance;
        private GameObject mMouseControlView;

        //banner控制的返回列表
        private LinkedList<BaseWindow> mQueueWindowList = new LinkedList<BaseWindow>();

        public void OpenUIWithParamFromLua(int viewID, object obj)
        {
            OnOpenUI(viewID, obj);
        }

        public void OpenUIFromLua(string windowName)
        {
            OnOpenUI(mBaseWindowIDDic[windowName], null);
        }

        public int RegisterView(Type baseWindow)
        {
            mBaseWindowTypeDic[++mViewIndex] = baseWindow;
            mBaseWindowNameDic[mViewIndex] = baseWindow.Name;
            mBaseWindowIDDic[baseWindow.Name] = mViewIndex;

            return mViewIndex;
        }

        public int GetViewID(string viewName)
        {
            return mBaseWindowIDDic[viewName];
        }

        private void SetFontDepth(GameObject go)
        {
         /*   UILabel[] labels = go.GetComponentsInChildren<UILabel>();
            for (int i = 0, count = labels.Length; i < count; i++)
            {
                labels[i].depth = AssetUnit.sDefaultDepth;
            }*/
        }
        //TODO UseSyncLoaderManagerLoad
        public void LoadView(string path, string viewName, Action<GameObject> callBack)
        {
        }

        public void Init()
        {
            LoadView("UI/ViewRoot/ViewRoot", "ViewRoot", SetRoot);
        }

        private void SetRoot(GameObject go)
        {
            mViewRoot = go;
            GameObject.DontDestroyOnLoad(mViewRoot);

            mTransform = mViewRoot.transform;
           
        }

        private GameObject AddLayerContainer(string containerName)
        {
            GameObject child = new GameObject(containerName);
            GameObjectUtil.AddChild(mViewRoot, child);
            child.transform.localScale = Vector3.one;

            return child;
        }

        private GameObject AddLayer(GameObject parent, RenderQueueEnum effectLayerEnum)
        {
            GameObject child = new GameObject(effectLayerEnum.ToString());
            GameObjectUtil.AddChild(parent, child);
            child.transform.localScale = Vector3.one;
         //   GameObjectUtil.SetLayer(child, LayerEnum.NGUI);
            return child;
        }

        private GameObject GetNormalLayer(GameObject parent, RenderQueueEnum effectLayerEnum)
        {
            GameObject go = AddLayer(parent, effectLayerEnum);
            mNormalLayerList.Add(go);
            return go;
        }

        private GameObject GetPopLayer(GameObject parent, RenderQueueEnum effectLayerEnum)
        {
            GameObject go = AddLayer(parent, effectLayerEnum);
            mPopLayerList.Add(go);
            return go;
        }

        private void OnToggleUI(int viewEnum, ToggleUIType type, object paramObject)
        {
            switch (type)
            {
                case ToggleUIType.toggleTrue:
                    OnOpenUI(viewEnum, paramObject);
                    break;
                case ToggleUIType.toggleFalse:

                    BaseWindow baseWindow = null;
                    if (mBaseWindowDic.TryGetValue(viewEnum, out baseWindow))
                    {
                        if (baseWindow.mIsLoadComplete && baseWindow.mIsShow)
                        {
                            baseWindow.HideView();
                        }

                        RemoveQueueWindow(baseWindow);
                    }

                    break;
                case ToggleUIType.toggleAuto:
                    OnSwitchUIVisible(viewEnum, paramObject);
                    break;
            }
        }

        private void OnSwitchUIVisible(int viewEnum, object paramObject)
        {
            BaseWindow baseWindow = GetRegisterView(viewEnum);

            if (baseWindow.mIsLoadComplete)
            {
                if (baseWindow.mIsShow)
                {
                    baseWindow.HideView();
                }
                else
                {
                    OnOpenUI(viewEnum, paramObject);
                }
            }
            else
            {
                OnOpenUI(viewEnum, paramObject);
            }
        }

        private void OnOpenUI(int viewEnum, object paramObject)
        {
            BaseWindow baseWindow = GetRegisterView(viewEnum);
            GameObject parentLayer = baseWindow.GetParentLayer();
            List<BaseWindow> windowList = GetBaseWindowsByParentLayer(parentLayer);
            BaseWindow otherWindow = null;

            for (int i = 0, count = windowList.Count; i < count; i++)
            {
                otherWindow = windowList[i];

                if (otherWindow != baseWindow && otherWindow.mIsShow)
                    break;
            }

            if (otherWindow == baseWindow)
                otherWindow = null;

            if (windowList.Contains(baseWindow) == false)
                windowList.Add(baseWindow);
            else
            {
                //避免快速点击重复打开界面
                if (baseWindow.mViewParam.openInShortTime == false && Time.frameCount - baseWindow.mLastShowFrameCount < 10)
                    return;
            }

            baseWindow.mLastShowFrameCount = Time.frameCount;

            bool hideOtherWindow = otherWindow != null && baseWindow.mViewParam.forbidReplacePrevView == false;

            if (baseWindow.mIsLoadComplete)
            {
                if (hideOtherWindow)
                    otherWindow.HideView();

                baseWindow.ShowView(paramObject);
            }
            else
            {
                baseWindow.mShowHandler += delegate()
                {
                    if (hideOtherWindow)
                        otherWindow.HideView();
                };

                baseWindow.ShowView(paramObject);
            }
        }

        public BaseWindow GetRegisterView(int viewID)
        {
            BaseWindow baseWindow;

            if (!mBaseWindowDic.TryGetValue(viewID, out baseWindow))
            {
                Type t = mBaseWindowTypeDic[viewID];
                baseWindow = Activator.CreateInstance(t) as BaseWindow;
                mBaseWindowDic[viewID] = baseWindow;

                baseWindow.mViewID = viewID;
            }

            return baseWindow;
        }

        private List<BaseWindow> GetBaseWindowsByParentLayer(GameObject parentLayer)
        {
            List<BaseWindow> windowList = null;

            if (!mBaseWindowParentDic.TryGetValue(parentLayer, out windowList))
            {
                windowList = new List<BaseWindow>();

                mBaseWindowParentDic[parentLayer] = windowList;
            }

            return windowList;
        }

        private bool HideLayer(GameObject layer)
        {
            List<BaseWindow> windowList = GetBaseWindowsByParentLayer(layer);

            for (int i = 0, count = windowList.Count; i < count; i++)
            {
                BaseWindow baseWindow = windowList[i];

                if (baseWindow.mIsShow)
                {
                    baseWindow.HideView();

                    return true;
                }
            }

            return false;
        }

        public BaseWindow GetHideRebuildWindowOnLayer(GameObject layer)
        {
            List<BaseWindow> windowList = GetBaseWindowsByParentLayer(layer);

            for (int i = 0, count = windowList.Count; i < count; i++)
            {
                BaseWindow baseWindow = windowList[i];

                if (baseWindow.mIsShow == false)
                {
                    return baseWindow;
                }
            }
            return null;
        }

        //获取指定层级的显示窗口
        public BaseWindow GetShowWindowOnLayer(GameObject layer)
        {
            List<BaseWindow> windowList = GetBaseWindowsByParentLayer(layer);

            for (int i = 0, count = windowList.Count; i < count; i++)
            {
                BaseWindow baseWindow = windowList[i];

                if (baseWindow.mIsShow)
                {
                    return baseWindow;
                }
            }
            return null;
        }

        public BaseWindow GetTopShowWindowOnNormalLayers()
        {
            BaseWindow showWindow = null;

            for (int i = mNormalLayerList.Count - 1; i >= 0; i--)
            {
                showWindow = GetShowWindowOnLayer(mNormalLayerList[i]);

                if (showWindow != null && showWindow.mIsShow)
                {
                    return showWindow;
                }
            }

            return null;
        }

        public List<BaseWindow> GetUnderNormalLayerWindows(GameObject layer)
        {
            List<BaseWindow> list = new List<BaseWindow>();

            for (int i = 0, count = mNormalLayerList.Count; i < count; i++)
            {
                GameObject underLayer = mNormalLayerList[i];
                if (underLayer == layer)
                    break;

                BaseWindow underWnd = GetShowWindowOnLayer(underLayer);
                if (underWnd != null && underWnd.mIsShow)
                    list.Add(underWnd);
            }

            return list;
        }

        public void SaveRebuildView()
        {
            SaveHideRebuildView();
            SaveShowRebuildView();
        }

        private List<KeyValuePair<int, object>> mHideRebuildViewList = new List<KeyValuePair<int, object>>();
        private void SaveHideRebuildView()
        {
            mHideRebuildViewList.Clear();
            BaseWindow showWindow = null;

            for (int i = 0, count = mNormalLayerList.Count; i < count; i++)
            {
                showWindow = GetHideRebuildWindowOnLayer(mNormalLayerList[i]);

                if (showWindow != null && mQueueWindowList.Contains(showWindow))
                {
                    KeyValuePair<int, object> kvp = new KeyValuePair<int, object>(showWindow.mViewID, showWindow.GetRebuildParam());
                    mHideRebuildViewList.Add(kvp);
                }
            }
        }

        private List<KeyValuePair<int, object>> mShowRebuildViewList = new List<KeyValuePair<int, object>>();
        private void SaveShowRebuildView()
        {
            mShowRebuildViewList.Clear();
            BaseWindow showWindow = null;

            for (int i = 0, count = mNormalLayerList.Count; i < count; i++)
            {
                showWindow = GetShowWindowOnLayer(mNormalLayerList[i]);

                if (showWindow != null && mQueueWindowList.Contains(showWindow))
                {
                    KeyValuePair<int, object> kvp = new KeyValuePair<int, object>(showWindow.mViewID, showWindow.GetRebuildParam());
                    mShowRebuildViewList.Add(kvp);
                }
            }
        }

        public void OpenRebuildView()
        {
            OpenHideRebuildView();

            OpenShowRebuildView();
        }

        private void OpenHideRebuildView()
        {
            for (int i = 0, count = mHideRebuildViewList.Count; i < count; i++)
            {
                KeyValuePair<int, object> kvp = mHideRebuildViewList[i];

                OnOpenUI(kvp.Key, kvp.Value);
            }

            mHideRebuildViewList.Clear();
        }

        private void OpenShowRebuildView()
        {
            for (int i = 0, count = mShowRebuildViewList.Count; i < count; i++)
            {
                KeyValuePair<int, object> kvp = mShowRebuildViewList[i];

                OnOpenUI(kvp.Key, kvp.Value);
            }

            mShowRebuildViewList.Clear();
        }

        public void HideAllNormalPopLayerWindow()
        {
            BaseWindow showWindow = null;

            for (int i = 0, count = mNormalLayerList.Count; i < count; i++)
            {
                showWindow = GetShowWindowOnLayer(mNormalLayerList[i]);

                if (showWindow != null && showWindow.mIsShow)
                    showWindow.HideView();
            }

            //pop层也要关闭，列如召唤师升级界面
            for (int i = 0, count = mPopLayerList.Count; i < count; i++)
            {
                showWindow = GetShowWindowOnLayer(mPopLayerList[i]);

                if (showWindow != null && showWindow.mIsShow)
                    showWindow.HideView();
            }

            ClearQueueWinowList();
        }

        public void InitCommonBanner()
        {
           /* if (mCommonBanner == null)
            {
                mCommonBanner = new CommonBanner();
                mCommonBanner.PreloadView();
            }*/
        }

        public void ClearQueueWinowList()
        {
            mQueueWindowList.Clear();
        }

        public void AddQueueWindow(BaseWindow queueWindow)
        {
           // if (queueWindow.mViewParam.replacePrevQueueWin)
            //    ClearQueueWinowList();

            //if (mQueueWindowList.Contains(queueWindow) == false)
            if (mQueueWindowList.Count > 0 && mQueueWindowList.Last.Previous != null && mQueueWindowList.Last.Previous.Value == queueWindow)
            {
                BaseWindow current = mQueueWindowList.Last.Value;
                mQueueWindowList.RemoveLast();
                mQueueWindowList.AddBefore(mQueueWindowList.Last, current);
            }
            else
            {
                mQueueWindowList.AddLast(queueWindow);
            }
        }

        public void RemoveQueueWindow(BaseWindow queueWindow)
        {
            mQueueWindowList.Remove(queueWindow);
        }

        public void ReturnLastQueueWindow(bool check)
        {
            BaseWindow baseWindow = UIManager.Instance.GetTopShowWindowOnNormalLayers();

         /*   if (baseWindow != null)
            {
                if (!check || baseWindow.OnReturnTriggerHideView())
                {
                    baseWindow.OnReturnHideView();
                    baseWindow.HideView();

                    if (baseWindow is BaseWindowWithBanner == false)
                        return;
                }
                else
                    return;
            }*/

            //关闭当前界面
            if (mQueueWindowList.Count > 0)
            {
                BaseWindow curShowWindow = mQueueWindowList.Last.Value;
                mQueueWindowList.RemoveLast();

                curShowWindow.HideView();
            }

            //显示上一界面
            if (mQueueWindowList.Count > 0)
            {
                BaseWindow nextShowWindow = mQueueWindowList.Last.Value;
                mQueueWindowList.RemoveLast();

                nextShowWindow.ShowView();
                nextShowWindow.OnReturnShowView();
            }
        }
        

        //0 非全屏
        //1 全屏，透明背景
        //2 全屏，不透明背景
        int IsFullScreenWindow(BaseWindow baseWindow)
        {
            BaseViewParam viewParam = baseWindow.mViewParam;

            if (string.IsNullOrEmpty(viewParam.bannerName) == false)
            {
                return 2;
            }

            int bgEnum = (int)viewParam.bgEnum;
            if (bgEnum != 0)
            {
              /*  if (bgEnum >= (int)BgEnum.BG1)
                {
                    return 2;
                }*/

                if (bgEnum >= (int)BgEnum.TRANSPARENT_CLICKABLE)
                    return 1;
            }

            return 0;
        }

        int fullScreenValue1 = 0;
        int fullScreenValue2 = 0;

        void ShowFullScreenWindow(int value)
        {
            if (value <= 0)
                return;

            if (value == 1)
            {
                fullScreenValue1++;
            }
            else if (value == 2)
            {
                fullScreenValue2++;
            }

            FullScreenWindowResult();
        }

        void HideFullScreenWindow(int value)
        {
            if (value <= 0)
                return;

            if (value == 1)
            {
                if (fullScreenValue1 > 0)
                    fullScreenValue1--;
            }
            else if (value == 2)
            {
                if (fullScreenValue2 > 0)
                    fullScreenValue2--;
            }

            FullScreenWindowResult();
        }

        void FullScreenWindowResult()
        {
            bool visible = true;
            bool cameraVisible = true;

            if (fullScreenValue2 > 0)
            {
                cameraVisible = false;
                visible = false;
            }
            else if (fullScreenValue1 > 0)
            {
                //visible = false;
            }

            if (cameraVisible == false)
            {
                SceneManager.Instance.SetCameraLayer(0);
            }
            else
            {
                if (visible)
                {
                    SceneManager.Instance.SetCameraNormalLayer();
                }
                else
                {
                    Debug.LogWarning("SetCameraLayer of Other");
                  //  SceneManager.Instance.SetCameraCullMainActor();
                }
            }
            mPlayerInfoView.SetActive(visible);
            SetMainInterfaceActive(cameraVisible);
        }

        public Action OnMainInterfaceActive;
        public bool mMainInterfaceVisible;
        GameTimer mMainInterfaceActiveTimer;
        void SetMainInterfaceActive(bool visible)
        {
            mMainInterfaceVisible = visible;
            if (mMainInterfaceActiveTimer != null)
            {
                mMainInterfaceActiveTimer.Dispose();
                mMainInterfaceActiveTimer = null;
            }

            mMainInterface.SetActive(visible);

            if (visible && OnMainInterfaceActive != null)
            {
                mMainInterfaceActiveTimer = GameTimer.ExecuteTotalFrames(2, null, delegate()
                {
                    if (mMainInterfaceVisible)
                    {
                        OnMainInterfaceActive();
                        OnMainInterfaceActive = null;
                    }
                }, true);
            }
        }
        
        //当某个层级有界面显示的时候调用这个函数
        public void OnLayerShow(GameObject go, BaseWindow baseWindow)
        {
            if (go == mLoginLayer)
            {
                mLogin.SetActive(true);
            }

            int fullScreenValue = IsFullScreenWindow(baseWindow);

            if (fullScreenValue > 0)
            {
              
            }
        }

        //当某个层级有界面隐藏的时候调用这个函数
        public void OnLayerHide(GameObject go, BaseWindow baseWindow)
        {
            int fullScreenValue = IsFullScreenWindow(baseWindow);

            if (fullScreenValue > 0)
            {
                
            }
        }

        public bool CheckShowFullScreenWindow()
        {
            foreach (KeyValuePair<int, BaseWindow> kvp in mBaseWindowDic)
            {
                if (kvp.Value.mIsShow && IsFullScreenWindow(kvp.Value) > 0)
                {
                    //Debug.LogError("IsFullScreenWindow show:" + kvp.Value);
                    return true;
                }
            }

            return false;
        }

        GameTimer mCheckFullScreenWindowTimer;

        //当场景切换的时候需要显示隐藏一些互斥层级
        public void ChangeLayerVisible(bool normal, bool battle)
        {
            mNormal.SetActive(normal);
            mBattle.SetActive(battle);

            mLogin.SetActive(false);
        }

        //释放指定层级下的所有界面
        //forceDispose为true时强制释放，为false时当界面是隐藏状态下才释放
        public void DisposeLayerViews(GameObject layer, bool forceDispose = false)
        {
            List<BaseWindow> windowList = GetBaseWindowsByParentLayer(layer);

            for (int i = windowList.Count - 1; i >= 0; i--)
            {
                BaseWindow baseWindow = windowList[i];

                if (forceDispose || baseWindow.mIsShow == false)
                {
                    if (baseWindow.mDisposeRemoveViewID)
                        mBaseWindowDic.Remove(baseWindow.mViewID);

                    windowList.RemoveAt(i);

                    baseWindow.CloseView();
                }
            }
        }
        
        //战斗场景转普通场景时的释放
        public void BattleToNormalDispose()
        {
            DisposeBigLayer(mBattle, true);
            DisposePopLayer();
        }

    

        private void DisposePopLayer()
        {
            for (int i = 0, count = mPopLayerList.Count; i < count; i++)
            {
                DisposeLayerViews(mPopLayerList[i], true);
            }
        }

        //释放大层级
        private void DisposeBigLayer(GameObject go, bool forceDispose = false)
        {
            Transform transform = go.transform;

            for (int i = 0, count = transform.childCount; i < count; i++)
            {
                DisposeLayerViews(transform.GetChild(i).gameObject, forceDispose);
            }
        }
        
        //过场动画时屏蔽所有UI. 可能在主城或者战斗场景
        public void OnCGEnter()
        {
            mJoystick.SetActive(false);
            mBattle.SetActive(false);
            mGlobal.SetActive(false);

            GlobalClickEnable(false);
        }
        

        private bool mGlobalClickEnable = false;
        //控制全局鼠标点击
        public void GlobalClickEnable(bool enable)
        {
            if (mLockGlobalClickEnable)
                return;

            if (enable == mGlobalClickEnable)
                return;

            mGlobalClickEnable = enable;
            mMouseControlView.SetActive(!enable);

            Debug.Log("GlobalClickEnable:" + enable);
        }

        private bool mLockGlobalClickEnable = false;
        public void LockSetGlobalClickEnable(bool lockSet)
        {
            mLockGlobalClickEnable = lockSet;

            Debug.Log("LockSetGlobalClickEnable:" + lockSet);
        }
    }
}
