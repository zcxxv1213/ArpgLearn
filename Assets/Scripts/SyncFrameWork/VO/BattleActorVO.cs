using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        }
    }
}
