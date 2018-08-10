using Google.Protobuf.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

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
        public void InitActors(RepeatedField<ActorVo> actorVos)
        {
            for (int i = 0; i < actorVos.Count; i++)
            {
                actorVODic.Add(actorVos.array[i].PlayerId, new BattleActorVO(actorVos.array[i]));
            }
        }
    }
}
