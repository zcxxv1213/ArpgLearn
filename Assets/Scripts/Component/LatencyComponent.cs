using Com.Game.Utils.Timers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ETModel
{
    [ObjectSystem]
    public class LatencyComponentAwakeSystem : AwakeSystem<LatencyComponent>
    {
        public override void Awake(LatencyComponent self)
        {
            self.Awake();
        }
    }
    public class LatencyComponent:Component
    {
        int messageNum;
        long mAddTime;
        long mNowAverageLatency;
        public void Awake()
        {
            messageNum = 0;
            mAddTime = 0;
            mNowAverageLatency = 0;
            GameTimer.SetInterval(1, () => CaculateLatency());
        }
        private void CaculateLatency()
        {
            mNowAverageLatency = mAddTime / messageNum;
            Debug.Log("当前1s内平均服务器到客户端延时为:  " + mNowAverageLatency);
            messageNum = 0;
            mAddTime = 0;
        }

        public long GetNowLatency()
        {
            return mNowAverageLatency;
        }
        public void AddAMsgLan(long t)
        {
            //注意如果t是毫秒
            Debug.Log(t);
            mAddTime += t;
        }
    }
}
