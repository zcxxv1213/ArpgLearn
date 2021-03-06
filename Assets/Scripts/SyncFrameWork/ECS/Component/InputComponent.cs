﻿using RollBack.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
namespace ETModel
{
    [ObjectSystem]
    public class InputComponentUpdateSystem : UpdateSystem<InputComponent>
    {
        public override void Update(InputComponent self)
        {
            self.Update();
        }
    }
    public class InputComponent:Component
    {
        InputState mInputState;
        //TODO加限制不可以一直做检测
        public void Update()
        {
            ETModel.Game.Scene.GetComponent<BattleControlComponent>().GetMainUnit().AddInputStateWithFrame(InputHelper.GetInputStateByOperation());
            /*if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                mInputState = InputState.Input0;
                
               // ETModel.Game.Scene.GetComponent<ETModel.SessionComponent>().Session.Send(new C2SOnlyInputState { Inputstate = (int)mInputState });
            }*/
        }
    }
}
