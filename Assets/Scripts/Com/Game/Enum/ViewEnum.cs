using Assets.Scripts.Com.Game.Manager;
using Assets.Scripts.Com.Game.Module.Login;

namespace Assets.Scripts.Com.Game.Enum
{
    public class ViewEnum
    {
        private static UIManager mUIManager = UIManager.Instance;      

        //登录界面;
        public static readonly int LoginView = mUIManager.RegisterView(typeof(LoginView));
  

    }
}
