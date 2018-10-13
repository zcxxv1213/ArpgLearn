using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ETModel
{
    [ObjectSystem]
   public class TimeTrackerComponentAwakeSystem : AwakeSystem<TimeTrackerComponent>
   {
       public override void Awake(TimeTrackerComponent self)
       {
           self.Awake();
       }
   }
    public class TimeTrackerComponent:Component
    {
        private long initialTime;
        private double frequency;
        public void Awake()
        {
            initialTime = Stopwatch.GetTimestamp();
            frequency = 1.0 / (double)Stopwatch.Frequency;
        }
        /// <summary>
        /// 自从开始经过的时间
        /// </summary>
        public double GetNowTime()
        {
            return (double)(Stopwatch.GetTimestamp() - initialTime) * frequency;
        }
    }
}
