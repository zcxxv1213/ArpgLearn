using Assets.Scripts.Com.Game.Events;
using UnityEngine;
using Com.Game.Core;
using Assets.Scripts.Com.Game.Enum;
using Assets.Scripts.Com.Game.Module.Scene;
using Assets.Scripts.Com.Game.Manager;
using Com.Game.Utils.Timers;

namespace Assets.Scripts.Com.Manager
{
    class SceneManager : Singleton<SceneManager>
    {
        public int mSceneId = -1;
        public int mSceneIDForLoader = -1;
        private const int mMainSceneID = 1;
        private const int mLoginSceneID = 0;
        protected EnterSceneStateEnum mEnterSceneState = EnterSceneStateEnum.INIT;

        //上一个场景
        public BaseScene mPrevScene;

       //当前场景
        protected BaseScene mCurScene;

        //副本
        public string mDungeonScene = "";

        public void Init()
        {
            EventDispatcher.Instance.AddEventListener(EventConstant.PRE_LOADING_DONE, OnPreloadingComplete);
            EventDispatcher.Instance.AddEventListener<int>(EventConstant.ASK_FOR_ENTER_SCENE, EnterSceneById);
        }

        private void OnPreloadingComplete()
        {
            this.BeforeEnterScene(mCurScene.mSceneId);
        }

        private void BeforeEnterScene(int sceneID)
        {
            Debug.Log("Release memory");
            int preID = 0;
            bool enterBattle = false;
            bool leaveBattle = false;
            bool reLogin = false;
            if (mPrevScene!=null)
            {
                preID = mPrevScene.mSceneId; 
            }
            if (preID > mMainSceneID)
            {
                leaveBattle = true;
            }
            if (sceneID > mMainSceneID)
            {
                enterBattle = true; 
            }
            if (mPrevScene != null && sceneID == mLoginSceneID)
                reLogin = true;

            if (mPrevScene != null)
                mPrevScene.BeforeExitScene();

            MemoryManager.Instance.BeforeEnterScene(reLogin, enterBattle, leaveBattle);
            mCurScene.BeforeEnterScene();
        }
        public EnterSceneStateEnum EnterSceneState
        {
            get { return mEnterSceneState; }
            set
            {
                mEnterSceneState = value;
                EventDispatcher.Instance.Dispatch<EnterSceneStateEnum>(EventConstant.ENTER_SCENE_STATE, mEnterSceneState);
            }
        }

        public void LoadSceneComplete()
        {
            UIManager.Instance.ClearQueueWinowList();
            PreloadManager.Instance.Init();

            if (mPrevScene != null)
            {
                mPrevScene.ExitScene();
                //mPrevScene = null;
            }

            mCurScene.EnterScene();
        }

        public BaseScene GetPreScene()
        {
            return mPrevScene;
        }

        public void EnterSceneById(int sceneId)
        {
            EventDispatcher.Instance.Dispatch<bool>(EventConstant.SHOW_REQUEST_LOADING, false);

            if (mCurScene != null)
            {
                if (mSceneId == sceneId)
                {
                    if (mCurScene.ReplaceScene() == false)
                        return;
                }

                mPrevScene = mCurScene;
                mPrevScene.BeforeExitScene();

                GameTimerManager.Instance.ClearOnChangeScene();
            //    MsgManager.ClearAction();
            }

            mSceneId = sceneId;
            switch (sceneId)
            {
                case mLoginSceneID:
                    mCurScene = new BaseScene(0);//TODO LoginScene;
                    break;
                case mMainSceneID:
                    mCurScene = new BaseScene(0);//TODO MainScene;
                    break;
                default:
                    break;
            }
            Debug.Log("sceneID:" + sceneId);
            // mCurScene.BeforeEnterScene();
            this.BeforeEnterScene(sceneId);
            mSceneIDForLoader = mSceneId;
        }

        public void SetCameraNormalLayer()
        {
        //    Camera.main.cullingMask |= 1 << LayerEnum.ACTOR | 1 << LayerEnum.gs;
        }
        
        public void SetCameraLayer(int layer)
        {
            Camera.main.cullingMask = layer;
        }
        //TODO Check
        public bool CheckIsLoginScene()
        {
            return mSceneId == 0;
        }

        public static bool CheckIsLoginScene(int id)
        {
            return id == 0;
        }
        public bool IsMainScene()
        {
            return CheckIsMainScene(mSceneId);
        }
        public static bool CheckIsMainScene(int id)
        {
            return id == 1;
        }
     
        public BaseScene GetCurScene
        {
            get { return mCurScene; }
        }

    }
}
