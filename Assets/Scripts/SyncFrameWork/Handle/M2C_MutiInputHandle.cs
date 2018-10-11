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
            mScene.GetComponent<LatencyComponent>().AddAMsgLan(message.Time - TimeHelper.GetCurrentTimeUnix());
            Unit u;
            C2SCoalesceInput mC2SCoalesceInput = new C2SCoalesceInput();
            for (int i = 0; i < message.UnitID.Count; i++)
            {
                u = mScene.GetComponent<WorldManagerComponent>().mEntityList[0].GetUnitByID(message.UnitID[i]);
                if (u!=null)
                {
                    u.QueueMessage(message.MC2SCoalesceInputs[i]);
                }
            }
        }
    }
}
