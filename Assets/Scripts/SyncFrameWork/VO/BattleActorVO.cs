using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
namespace ETModel
{
    public class BattleActorVO
    {
        public long mPlayerId;
        public int mTeam;
        public string mName;
        public BattleActorVO(ActorVo v)
        {
            mPlayerId = v.PlayerId;
            mTeam = v.Team;
            mName = v.NickName;
            Debug.Log(mTeam);
            Debug.Log(mPlayerId);
            Debug.Log(mName);
        }
    }
}
