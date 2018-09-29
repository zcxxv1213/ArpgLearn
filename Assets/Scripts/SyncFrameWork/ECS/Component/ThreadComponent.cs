using System;
using System.Collections.Generic;
using System.Text;

namespace ETModel
{
    public class ThreadComponent : Component
    {
        private readonly Dictionary<long, ThreadEntity> mThreadEnties = new Dictionary<long, ThreadEntity>();

        public override void Dispose()
        {
            if (this.IsDisposed)
            {
                return;
            }
            base.Dispose();

            foreach (ThreadEntity unit in this.mThreadEnties.Values)
            {
                unit.Dispose();
            }
            this.mThreadEnties.Clear();
        }

        public void Add(ThreadEntity entity)
        {
            this.mThreadEnties.Add(entity.Id, entity);
        }

       /* public ThreadEntity Get(long id)
        {
            this.mThreadEnties.TryGetValue(id, out ThreadEntity entity);
            return entity;
        }*/
        public void Remove(long id)
        {
            ThreadEntity unit;
            this.mThreadEnties.TryGetValue(id, out unit);
            this.mThreadEnties.Remove(id);
            unit?.Dispose();
        }

        public void RemoveNoDispose(long id)
        {
            this.mThreadEnties.Remove(id);
        }
        public int Count
        {
            get
            {
                return this.mThreadEnties.Count;
            }
        }
    }
}
