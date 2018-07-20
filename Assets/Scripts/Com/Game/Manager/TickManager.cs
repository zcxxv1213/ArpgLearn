using Assets.Scripts.Com.Game.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Scripts.Com.Game.Manager
{
    //每一帧需要调用的函数在这里注册
    public class TickManager
    {
        private static LinkedList<ITick> ticks = new LinkedList<ITick>();
        private static LinkedList<IFixedTick> fixedTicks = new LinkedList<IFixedTick>();

        private static Dictionary<ITick, LinkedListNode<ITick>> ticksRecord = new Dictionary<ITick, LinkedListNode<ITick>>();
        private static Dictionary<IFixedTick, LinkedListNode<IFixedTick>> fixedTicksRecord = new Dictionary<IFixedTick, LinkedListNode<IFixedTick>>();

        //有些逻辑需要放到LateUpdate里面更新
        private static LinkedList<ILateTick> lateTicks = new LinkedList<ILateTick>();
        private static Dictionary<ILateTick, LinkedListNode<ILateTick>> lateTicksRecord = new Dictionary<ILateTick, LinkedListNode<ILateTick>>();

        public static void AddLateTick(ILateTick tick)
        {
            if (!lateTicksRecord.ContainsKey(tick))
            {
                lateTicksRecord[tick] = lateTicks.AddLast(tick);
            }
        }

        public static void RemoveLateTick(ILateTick tick)
        {
            LinkedListNode<ILateTick> node;

            if (lateTicksRecord.TryGetValue(tick, out node))
            {
                lateTicks.Remove(node);

                lateTicksRecord.Remove(tick);
            }
        }

        public static void LateTick()
        {
            LinkedListNode<ILateTick> node = lateTicks.First;

            while (node != null)
            {
                ILateTick tick = node.Value;
                node = node.Next;

                tick.OnLateTick();
            }
        }

        public static void AddTick(ITick tick)
        {
            if (!ticksRecord.ContainsKey(tick))
            {
                ticksRecord[tick] = ticks.AddLast(tick);
            }
        }

        public static void RemoveTick(ITick tick)
        {
            LinkedListNode<ITick> node;

            if (ticksRecord.TryGetValue(tick, out node))
            {
                ticks.Remove(node);

                ticksRecord.Remove(tick);
            }
        }

        public static void AddFixedTick(IFixedTick tick)
        {
            if (!fixedTicksRecord.ContainsKey(tick))
            {
                fixedTicksRecord[tick] = fixedTicks.AddLast(tick);
            }
        }

        public static void RemoveFixedTick(IFixedTick tick)
        {
            LinkedListNode<IFixedTick> node;

            if (fixedTicksRecord.TryGetValue(tick, out node))
            {
                fixedTicks.Remove(node);

                fixedTicksRecord.Remove(tick);
            }
        }

        public static void Execuse()
        {
            LinkedListNode<ITick> node = ticks.First;

            while (node != null)
            {
                ITick tick = node.Value;
                node = node.Next;

                tick.OnTick();
            }
        }

        public static void FixedTick()
        {
            LinkedListNode<IFixedTick> node = fixedTicks.First;

            while (node != null)
            {
                IFixedTick tick = node.Value;
                node = node.Next;

                tick.OnFixedTick();
            }
        }

        public static void Clear()
        {
            ticks.Clear();
            fixedTicks.Clear();
            ticksRecord.Clear();
            fixedTicksRecord.Clear();
            lateTicks.Clear();
            lateTicksRecord.Clear();
        }
    }
}
