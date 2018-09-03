using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ETModel
{
    [MessageHandler]
    public class M2C_UnitInitMsg : AMHandler<UnitInitMsg>
    {
        protected override void Run(Session session, UnitInitMsg message)
        {
            Debug.Log("Handle");
            UnitComponent unitComponent = ETModel.Game.Scene.GetComponent<UnitComponent>();
            for (int i = 0; i < message.Units.Count; i++)
            {
                if (unitComponent.Get(message.Units[i].Id) == null)
                {
                    //UpdateInfo;
                    continue;
                }
                else
                {
                    Unit u = (unitComponent.Get(message.Units[i].Id));
                    FrameMoveData d = ProtobufHelper.FromBytes(typeof(FrameMoveData), message.Units[i].MoveComponentBytes.ToByteArray(), 0, message.Units[i].MoveComponentBytes.ToByteArray().Length) as FrameMoveData;
                    Debug.Log(d.posX);
                }
            }
        }
    }
}
