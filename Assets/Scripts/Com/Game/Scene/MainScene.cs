using Assets.Scripts.Com.Game.Enum;
using Assets.Scripts.Com.Game.Events;
using Assets.Scripts.Com.Manager;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Assets.Scripts.Com.Game.Module.Scene
{
    public class MainScene : BaseScene
    {
        public MainScene(int sceneID) : base(sceneID)
        {
           // this.InternalLoadSceneCompleted();
        }
        public override void LoadScene()
        {
            this.InternalLoadSceneCompleted();
        }
        protected override void LoadSceneCompleted()
        {
            Debug.Log("LoadSceneCom");
            EventDispatcher.Instance.Dispatch(EventConstant.SCENE_LOAD_COMPLETE);
            SceneManager.Instance.EnterSceneState = mLoadSceneCompleteState;
        }

        public override void OnEnterScene()
        {
            EventDispatcher.Instance.Dispatch<int, object>(EventConstant.OPEN_UI_WITH_PARAM, ViewEnum.MainInterfaceView,
            new object[] { //ArenaModel.Instance.mSelfInfo, ArenaModel.Instance.mEnemyInfo, ViewEnum.LoginView

            });
            mUIManager.SetCameraClearFlags(CameraClearFlags.SolidColor);
        }
        public override void BeforeExitScene()
        {
            mUIManager.SetCameraClearFlags(CameraClearFlags.Depth);
            Debug.Log("ExitScene");   
        }
        
    }
}
