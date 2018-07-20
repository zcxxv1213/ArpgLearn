using Assets.Scripts.Com.Game.Enum;
using Assets.Scripts.Com.Game.Events;
using Assets.Scripts.Com.Game.Manager;
using Assets.Scripts.Com.Manager;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Com.Game.Module.Scene
{
    class BaseScene
    {
        public int mSceneId = 0;
        public float defaultFov { get; protected set; }
        public Vector3 defaultOffset { get; protected set; }

        protected static UIManager sUIManager = UIManager.Instance;

        protected BackgroundType mBackgroundType = BackgroundType.BG_1;

        protected string mSceneName;
        protected SceneEnum mType;
        protected EnterSceneStateEnum mLoadSceneCompleteState;

        public BaseScene()
        {

        }
        public int GetSceneID()
        {
            return mSceneId;
        }
        //场景特有技能列表
        public virtual List<int> GetSceneSkills() { return null; }
        public virtual bool CanMoveCameraWhenDead() { return false; }
        public virtual bool ShowLeaveBattle() { return false; }
        public virtual bool EarlySpawnMonster() { return false; }
        //重复进入相同场景时是否可以替换当前场景
        public virtual bool ReplaceScene()
        {
            return false;
        }
        //碰壁是否需要自动转向
        public virtual bool AutoTurn()
        {
            return true;
        }
        public virtual bool ShowHitEffect()
        {
            return true;
        }

        public virtual bool PreloadSound()
        {
            return false;
        }

        protected void InitSceneConfig()
        {
          
        }

        
        public virtual void EnterScene()
        {
        
        }
        public virtual void BeforeEnterScene()
        {
            InitSceneSettings();
            InternalLoadScene();
        }

        public virtual void InitSceneSettings()
        {
           // ActorVO.heroModelScale = 1.0f;
        }

        //新场景加载完成后，旧场景才叫退出
        public virtual void ExitScene()
        {

        }

        //退出场景前的处理
        public virtual void BeforeExitScene()
        {
           
        }

        private void InternalLoadScene()
        {
            SceneManager.Instance.EnterSceneState = EnterSceneStateEnum.INIT;

            LoadScene();
        }

        protected virtual void LoadScene()
        {
            LoadSceneByName(GetSceneName());
        }

        protected virtual string GetSceneName()
        {
            return mSceneName;
        }

        protected virtual void ShowSceneLoading()
        {
            EventDispatcher.Instance.Dispatch<bool, BackgroundType>(EventConstant.SHOW_SCENE_LOADING, true, mBackgroundType);
        }

        private void LoadSceneByName(string name)
        {
            ShowSceneLoading();
            BeforeLoadScene();
        }

        protected virtual void InternalLoadSceneCompleted()
        {
            SceneManager.Instance.LoadSceneComplete();

            LoadSceneCompleted();
        }

        protected virtual void BeforeLoadScene()
        {

        }

        protected virtual void LoadSceneCompleted()
        {
            EventDispatcher.Instance.Dispatch<bool, BackgroundType>(EventConstant.SHOW_SCENE_LOADING, false, mBackgroundType);

            SceneManager.Instance.EnterSceneState = mLoadSceneCompleteState;
        }

        //登录成功的处理
        public virtual void LoginSuccessHandler()
        {

        }
    }
}
