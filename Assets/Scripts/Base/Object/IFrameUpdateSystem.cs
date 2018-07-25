using System;

namespace ETModel
{
    public interface IFrameUpdateSystem
    {
        Type Type();
        void Run(object o);
    }

    public abstract class FrameUpdateSystem<T> : IFrameUpdateSystem
    {
        public void Run(object o)
        {
            this.FrameUpdate((T)o);
        }

        public Type Type()
        {
            return typeof(T);
        }

        public abstract void FrameUpdate(T self);
    }
}
