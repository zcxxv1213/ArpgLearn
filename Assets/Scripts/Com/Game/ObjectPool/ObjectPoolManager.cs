using Com.Game.Utils.Timers;
using UnityEngine;

namespace Com.Game.ObjectPool
{
    class ObjectPoolManager
    {
        public static readonly ObjectPool<RecycleMS> sMemoryStreamPool = new ObjectPool<RecycleMS>();
       // public static readonly ObjectPool<RawPacket> sRawPacketPool = new ObjectPool<RawPacket>();
        public static readonly ObjectPool<GameTimer> mGameTimerPool = new ObjectPool<GameTimer>();
      //  public static readonly ObjectPool<LuaRawPacket> mLuaRawPacketPool = new ObjectPool<LuaRawPacket>();
    }
}
