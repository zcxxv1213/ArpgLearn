using ETModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.SyncFrameWork.Handle
{
    [MessageHandler(AppType.Map)]
    public class M2C_CreatUnit_Handle : AMHandler<Actor_CreateUnits>
    {
        protected override void Run(Session session, Actor_CreateUnits message)
        {
            Debug.Log("RevMsg");
            Debug.Log(message.ActorId);
            Debug.Log(message.Units[0].UnitId);
        }
    }
}
