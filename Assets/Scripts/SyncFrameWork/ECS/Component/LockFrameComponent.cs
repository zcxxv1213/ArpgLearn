using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ETModel
{
    public enum FrameType {
        optimistic,
        predict
    }
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
        public int FrameRate = 32;
        public int InfluenceResolution = 2;
        public long DeltaTime = 0;
        public float DeltaTimeF = 0;
        public int Frame;
        public bool mStartGame = false;
        public FrameMessage FrameMessage;
        private FrameType mFrameType;

        private int InfluenceCount;
        public int InfluenceFrameCount { get; private set; }
        TimerComponent timerComponent = Game.Scene.GetComponent<TimerComponent>();
        public void StartGame()
        {
            mFrameType = FrameType.predict;
            this.mStartGame = true;
        }
        public void Awake()
        {
            Frame = 0;
            DeltaTime = FixedMath.One / FrameRate;
            DeltaTimeF = DeltaTime / FixedMath.OneF;
            FrameMessage = new FrameMessage() { Frame = Frame };
        }
        public async void Update()
        {
            if (mStartGame)
            {
                if (mFrameType == FrameType.optimistic)
                {
                    await timerComponent.WaitAsync(200);
                    ++Frame;
                }
                
            }
        }
        public void OnRecvFrameMessage()
        {

        }
    }
}