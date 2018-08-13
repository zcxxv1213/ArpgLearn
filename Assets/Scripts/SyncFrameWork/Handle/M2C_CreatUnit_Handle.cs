﻿using Assets.Scripts.SyncFrameWork.Factory;
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
        protected override async void Run(Session session, Actor_CreateUnits message)
        {
            UnitComponent unitComponent = ETModel.Game.Scene.GetComponent<UnitComponent>();

            for (int i = 0; i < message.Units.Count; i++)
            {
                if (unitComponent.Get(message.Units[i].UnitId) != null)
                {
                    //UpdateInfo;
                    continue;
                }
                else
                {
                    Unit unit = UnitFactory.Create(message.Units[i].UnitId,"TestName");
                    unit.Position = new Vector3(message.Units[i].X / 1000f, 0, message.Units[i].Z / 1000f);
                    unit.IntPos = new VInt3(message.Units[i].X, 0, message.Units[i].Z);
                }
            }
            //Add FrameCompoont SendStart proto
            Game.Scene.AddComponent<LockFrameComponent>();
            M2C_ReadyStartGame response = (M2C_ReadyStartGame)await ETModel.SessionComponent.Instance.Session.Call(new C2M_ReadyStartGame() { });
            Game.Scene.GetComponent<LockFrameComponent>().StartGame(); 
        }
    }
}
