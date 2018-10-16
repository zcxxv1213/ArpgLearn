using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ETModel {
    public static class WorldEntityFactory
    {
        public static WorldEntity Create()
        {
            WorldManagerComponent worldManagerComponent = ETModel.Game.Scene.GetComponent<WorldManagerComponent>();
            if (worldManagerComponent == null)
            {
                worldManagerComponent = ETModel.Game.Scene.AddComponent<WorldManagerComponent>();
            }
            WorldEntity worldEntity = ComponentFactory.Create<WorldEntity>();
            Debug.Log(worldManagerComponent == null);
            Debug.Log(worldEntity == null);
            worldManagerComponent.AddWorld(worldEntity);
            return worldEntity;
        }
    }
}

