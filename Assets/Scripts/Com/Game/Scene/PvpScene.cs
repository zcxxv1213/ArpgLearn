using Assets.Scripts.Com.Game.Mono;
using Assets.Scripts.Com.Manager;
using Com.Game.Manager;
using ETModel;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Assets.Scripts.Com.Game.Module.Scene
{
    public class PvpScene : BaseScene
    {
        public PvpScene(int sceneID) : base(sceneID)
        {
            // this.InternalLoadSceneCompleted();
        }
        public override void LoadScene()
        {
            SyncResourceManager.LoadSceneExternal(((MapConfig)ETModel.Game.Scene.GetComponent<ConfigComponent>().Get(typeof(MapConfig), mSceneId)).ResourcesID.ToString(), (object T1, object T2) =>
            {
                this.InternalLoadSceneCompleted();
            });
        }
        protected override void LoadSceneCompleted()
        {
            //   SceneManager.Instance.EnterSceneState = mLoadSceneCompleteState;
            this.OnEnterBattleScene();
        }
        private void OnEnterBattleScene()
        {
            WorldEntity worldEntity = WorldEntityFactory.Create();
          //  WorldEntity worldEntity = ComponentFactory.Create<WorldEntity>();
            ETModel.Scene mScene = ETModel.Game.Scene;
            mScene.AddComponent<BattleControlComponent>().CreatUnitModel(mScene.GetComponent<UnitComponent>().GetAll());
            worldEntity.SetMainUnit(mScene.GetComponent<BattleControlComponent>().GetMainUnit());
          //  mScene.GetComponent<WorldManagerComponent>().AddWorld(worldEntity);
            foreach (var v in mScene.GetComponent<UnitComponent>().GetAll())
            {
                ETModel.Game.Scene.GetComponent<WorldManagerComponent>().AddUnitToWorld(v, worldEntity);
            }
            ThreadEntity threadEntity = ThreadEntityFactory.Create();
            threadEntity.InitWorldEntity(worldEntity);
            CameraController.Instance.OnEnterBattleScene();
        }
        public override void BeforeExitScene()
        {
            CameraController.Instance.OnExitBattleScene();
        }
    }

}