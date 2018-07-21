using UnityEngine;
using Assets.Scripts.Com.Game.Core;
using Assets.Scripts.Com.Game.Manager;
using Assets.Scripts.Com.Game.Enum;
using Assets.Scripts.Com.Game.Events;

namespace Assets.Scripts.Com.Game.Module.Login
{
    class LoginView : BaseWindow
    {
        protected override BaseViewParam InitViewParam()
        {
            return new BaseViewParam()
            {
                url = "ui/login/",
                viewName = "login_view",
                ParentLayer = UIManager.Instance.mLoginLayer,
                bgEnum = BgEnum.NONE,
                forbidSound = true,
                openInShortTime = true
            };
        }

        protected override void Init()
        {
            base.Init();
          
        }
   
        protected override void OnViewShow()
        {
          
        }
        

        protected override void OnViewShowWithParam(object param)
        {
            base.OnViewShowWithParam(param);
        }
    }
}
