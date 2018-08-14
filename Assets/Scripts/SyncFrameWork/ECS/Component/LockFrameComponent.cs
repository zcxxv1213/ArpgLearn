using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ETModel
{
    public struct SessionFrameMessage
    {
        public Session Session;
        public FrameMessage FrameMessage;
    }
    [ObjectSystem]
    public class LockFrameComponentUpdateSystem : UpdateSystem<LockFrameComponent>
    {
        public override void Update(LockFrameComponent self)
        {
            self.Update();
        }
    }
    [ObjectSystem]
    public class LockFrameComponentUpdateAwakeSystem : AwakeSystem<LockFrameComponent>
    {
        public override void Awake(LockFrameComponent self)
        {
            self.Awake();
        }
    }

    public class LockFrameComponent : Component
    {
        public int Frame;
        private bool mStartGame = false;
        public FrameMessage FrameMessage;
        TimerComponent timerComponent = Game.Scene.GetComponent<TimerComponent>();
        public void StartGame()
        {
            this.mStartGame = true;
        }
        public void Awake()
        {
            Frame = 0;
            FrameMessage = new FrameMessage() { Frame = Frame };
        }
        public async void Update()
        {
            if (mStartGame)
            {
           //     Debug.Log("Start");
                await timerComponent.WaitAsync(200);
                ++Frame;
            }

        }

    }
}