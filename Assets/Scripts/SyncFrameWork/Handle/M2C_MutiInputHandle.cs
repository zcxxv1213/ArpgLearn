using ETModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.SyncFrameWork.Handle
{
    [MessageHandler]
    public class M2C_MutiInputHandle : AMHandler<S2CCoalesceInput>
    {
        protected override void Run(Session session, S2CCoalesceInput message)
        {
            ETModel.Scene mScene = ETModel.Game.Scene;
            mScene.GetComponent<WorldManagerComponent>().mEntityList[0].mUnitList
        }
    }
}
