using Assets.Scripts.Com.Game.Events;
using UnityEngine;
using Com.Game.Core;
using Assets.Scripts.Com.Game.Enum;

namespace Assets.Scripts.Com.Manager
{
    class SceneManager : Singleton<SceneManager>
    {
        public int mSceneId = -1;
        public int mSceneIDForLoader = -1; 
        protected EnterSceneStateEnum mEnterSceneState = EnterSceneStateEnum.INIT;

        //上一个场景
        public BaseScene mPrevScene;

       //当前场景
        protected BaseScene mCurScene;

        //副本
        public string mDungeonScene = "";

        public void Init()
        {
            EventDispatcher.Instance.AddEventListener<int>(EventConstant.ASK_FOR_ENTER_SCENE, EnterSceneById);
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

        // 1 副本 2大乱斗 3 竞技场
        public bool IsInvalidSkillInCurrentScene(int mask)
        {
            if (IsDungeon()) return ((1 << 1) & mask) > 0;
            if (IsMelee() || IsMeleeDungeon()) return ((1 << 2) & mask) > 0;
            if (IsArena()) return ((1 << 3) & mask) > 0;

            return false;
        }

        public void LoadSceneComplete()
        {
            SetCameraActor();

            UIManager.Instance.ClearQueueWinowList();
            GuideManager.Instance.ClearComponents();
            PreloadManager.Instance.Init();

            if (mPrevScene != null)
            {
                mPrevScene.ExitScene();
                //mPrevScene = null;
            }

            mCurScene.EnterScene();

            //新手，征服之海开启雾效
            RenderSettings.fog = IsTutorialScene() || IsEscort();
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
              
                default:
                    break;
            }


            Debug.Log("sceneID:" + sceneId);
            mCurScene.mSceneId = sceneId;
            mCurScene.BeforeEnterScene();
            mSceneIDForLoader = mSceneId;
        }

        public CameraControl GetCameraControl()
        {
            Camera mainCamera = Camera.main;

            SetCameraNormalLayer();

            CameraControl cameraControl = mainCamera.GetComponent<CameraControl>();
            if (cameraControl == null)
            {
                cameraControl = mainCamera.gameObject.AddComponent<CameraControl>();
                SceneManager.Instance.SetCameraParam(cameraControl);
            }

            return cameraControl;
        }

        public void SetCameraNormalLayer()
        {
            SetCameraCullMainActor();
            Camera.main.cullingMask |= 1 << LayerEnum.ACTOR | 1 << LayerEnum.gs;
        }

        public void SetCameraCullMainActor()
        {
            SetCameraLayer((1 << LayerEnum.DEFAULT) | (1 << LayerEnum.T4M));
        }

        public void SetCameraActor()
        {
            Camera.main.cullingMask |= 1 << LayerEnum.ACTOR;
        }

        public void SetCameraLayer(int layer)
        {
            Camera.main.cullingMask = layer;
        }

        public void SetCameraParam(CameraControl control)
        {

            control.SetFieldOfView(40f);

            JoystickView.sDirection = 0;

            switch (mSceneId)
            {
                case SysSceneConst.GAME:
                    control.offset = new Vector3(0, 11.6f, -16f);
                    control.lookOffset = new Vector3(0, 1.2f, 0);
                    control.SetFieldOfView(40f);
                    break;
                default:
                    if (GamePlayControl.Instance.IsTestGamePlayScene())
                    {
                        GamePlayControl.Instance.mGame.SetCameraParam(control);
                    }
                    else if (mCurScene is PVPGame)
                    {
                        PVPGame curScene = mCurScene as PVPGame;
                        curScene.SetCameraParam(control);
                    }
                    else if (mCurScene is MeleeScene)
                    {
                        MeleeScene curScene = mCurScene as MeleeScene;
                        control.SetFieldOfView(curScene.GetFieldOfView());
                        control.offset = curScene.GetCameraParam();
                    }
                    else if (mCurScene is ArenaScene)
                    {
                        ArenaScene curScene = mCurScene as ArenaScene;
                        control.SetFieldOfView(curScene.GetFieldOfView());
                        control.offset = curScene.GetCameraParam();
                    }
                    else if (mCurScene is DungeonScene)
                    {
                        DungeonScene curScene = mCurScene as DungeonScene;
                        control.SetFieldOfView(curScene.GetFieldOfView());
                        control.offset = curScene.GetCameraParam();

                        if (RenderSettings.fog)
                            curScene.SetFog();

                        JoystickView.sDirection = curScene.mSysdungeon.joystick_direction;
                    }
                    break;
            }
        }

        public bool CheckIsLoginScene()
        {
            return mSceneId <= SysSceneConst.LOGIN;
        }

        public static bool CheckIsLoginScene(int id)
        {
            return id == SysSceneConst.LOGIN;
        }
        public bool IsMainScene()
        {
            return CheckIsMainScene(mSceneId);
        }
        public static bool CheckIsMainScene(int id)
        {
            return id == SysSceneConst.GAME;
        }

        public static bool CheckIsDungeon(int id)
        {
            return id > 100000 && id < 200000;
        }

        public bool IsMelee()
        {
            return mSceneId > 200000 && mSceneId < 300000;
        }
        public bool IsArena()
        {
            return mSceneId > 300000 && mSceneId < 400000;
        }
        public bool IsLeague()
        {
            return mSceneId > 400000 && mSceneId < 500000;
        }
        public bool IsMeleeDungeon()
        {
            return mSceneId > 500000 && mSceneId < 600000;
        }
        public bool IsTrain()
        {
            return mSceneId > 600000 && mSceneId < 610000;
        }
        public bool IsPeerage()
        {
            return mSceneId > 610000 && mSceneId < 620000;
        }
        public bool IsJungle()
        {
            return mSceneId > 700000 && mSceneId < 800000;
        }
        public bool IsEscort()
        {
            return mSceneId > 800000 && mSceneId < 900000;
        }
        public bool IsHell()
        {
            return mSceneId > 900000 && mSceneId < 1000000;
        }
        public bool IsPVP()
        {
            return mSceneId > 1000000 && mSceneId < 1100000;
        }
     
        public BaseScene GetCurScene
        {
            get { return mCurScene; }
        }

    }
}
