using Assets.Scripts.Com.Game.Core;
using Assets.Scripts.Com.Game.Enum;
using Assets.Scripts.Com.Game.Manager;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ETModel;

namespace Assets.Scripts.Com.Game.Module.MainInterface
{
    public class MainInterfaceView : BaseWindow
    {
        Button mButtonTextC2GSendMsg;
        protected override BaseViewParam InitViewParam()
        {
            return new BaseViewParam()
            {
                url = "ui/main_interface/",
                viewName = "main_interface_view",
                ParentLayer = UIManager.Instance.mMainLayer,
                bgEnum = BgEnum.NONE,
                forbidSound = true,
                openInShortTime = true
            };
        }
        protected override void Init()
        {
            base.Init();
            this.FindAndAddClickListener("Button_Test_C2G_Send_Msg", () => { OnClickTestC2GSendMsg(); }, null, 1);
            this.FindAndAddClickListener("Button_Test_C2G_Send_Msg", () => { OnClickTestC2GEnterMap(); }, null, 1);
        }
        private void OnClickTestC2GSendMsg()
        {
            ETModel.Game.Scene.GetComponent<ETModel.SessionComponent>().Session.Send(new C2G_SendMsg { Info = "HelloETSever" });
        }
        private async void OnClickTestC2GEnterMap()
        {
            G2C_EnterMap g2C_EnterMap = (G2C_EnterMap)await ETModel.Game.Scene.GetComponent<ETModel.SessionComponent>().Session.Call(new C2G_EnterMap { });
            if (g2C_EnterMap.Error != 0)
            {
                Debug.LogError("Error:" + g2C_EnterMap.Error);
            }
            Debug.Log(g2C_EnterMap.UnitId);
            //LoadScene - > Send LoadComplete -> Enter
        }
    }
}
