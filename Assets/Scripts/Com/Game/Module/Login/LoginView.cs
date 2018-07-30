using UnityEngine;
using Assets.Scripts.Com.Game.Core;
using Assets.Scripts.Com.Game.Manager;
using Assets.Scripts.Com.Game.Enum;
using Assets.Scripts.Com.Game.Events;
using UnityEngine.UI;

namespace Assets.Scripts.Com.Game.Module.Login
{
    class LoginView : BaseWindow
    {
        private InputField mInputEmail;
        private InputField mInputPass;
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
            mInputEmail = this.FindComponent<InputField>("InputField_email");
            mInputPass = this.FindComponent<InputField>("InputField_password");
            this.FindAndAddClickListener("Button_login", () => { OnClickLogin();  },null,1);
        }
   
        protected override void OnViewShow()
        {
          
        }
        private void OnClickLogin()
        {
            Debug.Log("Login");
        }

        protected override void OnViewShowWithParam(object param)
        {
            base.OnViewShowWithParam(param);
        }
    }
}
