using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Scripts.Com.Game.Core
{
    public interface IFixedTick
    {
        //固定时间更新的tick, 不受帧率影响, 在project Setting->Time中可以设置自己想要的固定刷新时间
        void OnFixedTick();
    }
}
