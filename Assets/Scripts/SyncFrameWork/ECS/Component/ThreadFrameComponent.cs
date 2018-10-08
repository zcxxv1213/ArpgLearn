using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using UnityEngine;

namespace ETModel
{
    [ObjectSystem]
    public class ThreadFrameAwakeComponentSystem : AwakeSystem<ThreadFrameComponent>
    {
        public override void Awake(ThreadFrameComponent self)
        {
            self.Awake();
        }
    }

    public class ThreadFrameUpdateComponentSystem : UpdateSystem<ThreadFrameComponent>
    {
        public override void Update(ThreadFrameComponent self)
        {
            self.Update();
        }
    }


    public class ThreadFrameComponent : Component
    {
        WorldEntity mWorldEntity;
        public static int s_intervalTime = 1000; //单位毫微秒
        public int FrameRate = 32;
        public int InfluenceResolution = 2;
        public long DeltaTime = 0;
        public float DeltaTimeF = 0;
        public int FrameCount;
        public bool mStartGame = false;
        private long _playRate = FixedMath.One;
        private int InfluenceCount;
        private long mStartTime;
        public int PauseCount { get; private set; }
        public bool IsPaused { get { return PauseCount > 0; } }
        TimeSpan mTimeSpan;
        float mTime;
        bool firstUpdate;
        double elapsedTime;
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
                    //Time.fixedDeltaTime = BaseDeltaTime / _playRate.ToFloat();
                }
            }
        }

        public void Awake()
        {
            this.InitComponent();
        }

        public void InitComponent()
        {
            FrameCount = 0;
            DeltaTime = FixedMath.One / FrameRate;
            Log.Info(DeltaTime.ToString());
            DeltaTimeF = (DeltaTime / FixedMath.OneF);

            Log.Info(DeltaTimeF.ToString());
            PlayRate = FixedMath.One;
            InfluenceCount = 0;
            PauseCount = 0;
            firstUpdate = true;
        }
        public void SetWorldEntity(WorldEntity w)
        {
            mWorldEntity = w;
        }

        private void UpdateWorld(double deltaTime)
        {
            //
            //Update RollBack System Send Input
            mWorldEntity.GetComponent<RollBack.RollbackDriver>().Update(deltaTime);
        }

        public void Update()
        {
            if (firstUpdate)
            {
                firstUpdate = false;
                elapsedTime = Time.time - mTime;
                mTime = Time.time;
                //Update;
                UpdateWorld(elapsedTime);
            }
            else
            {
                if ((Time.time - mTime) > s_intervalTime)
                {
                    //Update;
                    elapsedTime = Time.time - mTime;
                    mTime = Time.time;
                    UpdateWorld(elapsedTime);
                }
            }
        }
        public override void Dispose()
        {
            if (this.IsDisposed)
            {
                return;
            }

        }
    }
}
