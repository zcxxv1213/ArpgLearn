using Assets.Scripts.Com.Game.Core;
using ETModel;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Assets.Scripts.Com.Game.Module.Role
{
    public class RoleModel : BaseModel<RoleModel>
    {
        Player_Info_Base mPlayerInfoBase;
        public void OnRevInfo(Player_Info_Base info)
        {
            mPlayerInfoBase = info;
        }
        public long GetPlayerID()
        {
            return mPlayerInfoBase.PlayerId;
        }
    }
}