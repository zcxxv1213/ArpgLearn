using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ETModel
{
   /* [ObjectSystem]
    public class BattleDataComponentAwakeSystem : AwakeSystem<BattleDataComponent>
    {
        public override void Awake(BattleDataComponent self)
        {
            self.Awake();
        }
    }*/
    public class BattleDataComponent : Component
    {
        Dictionary<Int64, BattleActorVO> actorVODic = new Dictionary<long, BattleActorVO>();
       // List<act>
        public void Awake()
        {

        }
        public void InitActors(ActorVo[] actorVos)
        {
            foreach (var v in actorVos)
            {
                actorVODic[v.PlayerId] = new BattleActorVO(v);
            }
        }
    }
}
