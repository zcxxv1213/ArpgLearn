using Assets.Scripts.Com.Game.Manager;
using Assets.Scripts.Com.Game.Module.Login;
using Assets.Scripts.Com.Game.Module.MainInterface;

namespace Assets.Scripts.Com.Game.Enum
{
    public class ViewEnum
    {
        private static UIManager mUIManager = UIManager.Instance;      

        //登录界面;
        public static readonly int LoginView = mUIManager.RegisterView(typeof(LoginView));
        //主界面
        public static readonly int MainInterfaceView = mUIManager.RegisterView(typeof(MainInterfaceView));

    }
}
