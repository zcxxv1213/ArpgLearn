using Assets.Scripts.Com.Game.Enum;
using Assets.Scripts.Com.Game.Events;
using Assets.Scripts.Com.Game.Manager;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Assets.Scripts.Com.Game.Module.Scene
{
    public class LoginScene : BaseScene
    {
        public LoginScene(int sceneID) : base(sceneID)
        {

        }
        public override void LoadScene()
        {
            UIManager.Instance.SetCameraClearFlags(CameraClearFlags.SolidColor);
            EventDispatcher.Instance.Dispatch<int, object>(EventConstant.OPEN_UI_WITH_PARAM, ViewEnum.LoginView,
                new object[] { //ArenaModel.Instance.mSelfInfo, ArenaModel.Instance.mEnemyInfo, ViewEnum.LoginView

                });
         //   UIManager.Instance.LoadView()
        }
    }
}