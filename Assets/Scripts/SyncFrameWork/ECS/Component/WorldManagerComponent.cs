using System;
using System.Collections.Generic;
using System.Text;

namespace ETModel
{
    public class WorldManagerComponent : Component
    {
        Dictionary<long, WorldEntity> mWorldDic = new Dictionary<long, WorldEntity>();
        public List<WorldEntity> mEntityList = new List<WorldEntity>();

        public void AddWorld(WorldEntity entity)
        {
            mEntityList.Add(entity);
        }

        public void RemoveWorld()
        {
            //RemoveUnit - > Remove World
        }

        public WorldEntity GetWorldByUnit(Unit u)
        {
            WorldEntity w = null;
            if (mWorldDic.TryGetValue(u.Id, out w))
            {
                return w;
            }
            else
            {
                return null;
            }
        }

        public void AddUnitToWorld(Unit u, WorldEntity world)
        {
            mWorldDic.Add(u.Id, world);
            world.AddUnit(u);
        }
        public bool CheckUnitInWorld(Unit u)
        {
            WorldEntity entity;
            this.mWorldDic.TryGetValue(u.Id, out entity);
            if (entity != null)
                return true;
            return false;
        }
    }
}
