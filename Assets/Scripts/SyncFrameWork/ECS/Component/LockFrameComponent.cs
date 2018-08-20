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
    public class LockFrameComponentLateUpdateSystem : LateUpdateSystem<LockFrameComponent>
    {
        public override void LateUpdate(LockFrameComponent self)
        {
            self.LateUpdate();
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
        public int FrameCount;
        public bool mStartGame = false;
        public FrameMessage FrameMessage;
        private FrameType mFrameType;
        static bool Stalled;
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
            FrameCount = 0;
            DeltaTime = FixedMath.One / FrameRate;
            DeltaTimeF = DeltaTime / FixedMath.OneF;
            
            PlayRate = FixedMath.One;
            Time.timeScale = 1f;
            InfluenceCount = 0;
            this.SetUp();
          //  FrameMessage = new FrameMessage() { Frame = Frame };
        }
        private void SetUp()
        {
            Time.fixedDeltaTime = DeltaTimeF;
            Time.maximumDeltaTime = Time.fixedDeltaTime * 2;
            LSUtility.Initialize(1);
            Stalled = true;

        }

        private long _playRate = FixedMath.One;
        public long PlayRate
        {
            get
            {
                return _playRate;
            }
            set
            {
                if (value != _playRate)
                {
                    _playRate = value;
                    Time.timeScale = PlayRate.ToFloat();
                    //Time.fixedDeltaTime = BaseDeltaTime / _playRate.ToFloat();
                }
            }
        }

        public void Simulate()
        {

        }

        public void Update()
        {
            if (!mStartGame)
            {
                return;
            }
        }

        public void LateUpdate()
        {
            if (!mStartGame)
            {
                return;
            }
        }
        public void OnRecvFrameMessage()
        {

        }
    }
}