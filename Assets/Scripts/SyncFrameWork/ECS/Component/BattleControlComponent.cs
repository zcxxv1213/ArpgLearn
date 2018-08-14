using Assets.Scripts.Com.Game.Module.Role;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace ETModel
{
    public class BattleControlComponent : Component
    {
        Unit mMainUnit;
        public void CreatUnitModel(Unit[] units)
        {
            RoleModel roleModel = RoleModel.Instance;
            foreach (var v in units)
            {
                GameObject obj = new GameObject();
                obj.name = v.name;
                obj.transform.position = v.Position;
                v.GameObject = obj;
                if (v.mPlayerID == roleModel.GetPlayerID())
                {
                    Debug.Log("SetMainHero");
                    this.SetMainUnit(v); 
                }
            }
        }
        public void SetMainUnit(Unit u)
        {
            mMainUnit = u;
        }
        public Unit GetMainUnit()
        {
            return this.mMainUnit;
        }
    }
}
