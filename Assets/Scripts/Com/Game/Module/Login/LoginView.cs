using UnityEngine;
using Assets.Scripts.Com.Game.Core;
using Assets.Scripts.Com.Game.Manager;
using Assets.Scripts.Com.Game.Enum;
using Assets.Scripts.Com.Game.Events;
using UnityEngine.UI;
using ETModel;
using System.Net;

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
        private async void OnClickLogin()
        {
            Debug.Log("Login");
            Session session = ETModel.Game.Scene.GetComponent<SessionComponent>().Session;
            if (session != null)
            {
                R2C_Login r2C_Login = (R2C_Login)await session.Call(
                    new C2R_Login() { Account = mInputEmail.text, Password = mInputPass.text }
                    );
                session.Dispose();
                IPEndPoint connetEndPoint = NetworkHelper.ToIPEndPoint(r2C_Login.Address);
                Session gateSession = ETModel.Game.Scene.GetComponent<NetOuterComponent>().Create(connetEndPoint);
                ETModel.Game.Scene.AddComponent<ETModel.SessionComponent>().Session = gateSession;

                G2C_LoginGate g2CLoginGate = (G2C_LoginGate)await SessionComponent.Instance.Session.Call(new C2G_LoginGate() { Key = r2C_Login.Key });
                Debug.Log("LoginToGateServer");
                Player player = ETModel.ComponentFactory.CreateWithId<Player>(g2CLoginGate.PlayerId);
                PlayerComponent playerComponent = ETModel.Game.Scene.GetComponent<PlayerComponent>();
                playerComponent.MyPlayer = player;
                Debug.Log(player.UnitId);
            }
        }

        protected override void OnViewShowWithParam(object param)
        {
            base.OnViewShowWithParam(param);
        }
    }
}
