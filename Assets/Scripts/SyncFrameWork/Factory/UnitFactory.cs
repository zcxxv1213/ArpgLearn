using ETModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.SyncFrameWork.Factory
{
    public static class UnitFactory
    {
        public static Unit Create(long id,string name)
        {
          //  GameObject obj = new GameObject();
           // obj.name = name;
            UnitComponent unitComponent = Game.Scene.GetComponent<UnitComponent>();

            Unit unit = ComponentFactory.CreateWithId<Unit>(id);
            unit.name = name;
            //InistanceObj
       //     unit.GameObject = obj;
          //  GameObject parent = GameObject.Find($"/Global/Unit");
          //  unit.GameObject.transform.SetParent(parent.transform, false);
         //   unit.AddComponent<AnimatorComponent>();
            unit.AddComponent<MoveComponent>();

            unitComponent.Add(unit);
            return unit;
        }
    }
}
