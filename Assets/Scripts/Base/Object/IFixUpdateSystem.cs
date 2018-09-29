using System;

namespace ETModel
{
    public interface IFixUpdateSystem
    {
        Type Type();
        void Run(object o);
    }

    public abstract class FixUpdateSystem<T> : IFixUpdateSystem
    {
        public void Run(object o)
        {
            this.FixUpdate((T)o);
        }

        public Type Type()
        {
            return typeof(T);
        }

        public abstract void FixUpdate(T self);
    }
}
