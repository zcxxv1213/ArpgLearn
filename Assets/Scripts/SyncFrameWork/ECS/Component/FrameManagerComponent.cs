using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace ETModel {

    public class FrameManagerComponentAwakeSystem : AwakeSystem<FrameManagerComponent>
    {
        public override void Awake(FrameManagerComponent self)
        {
            self.Awake();
        }
    }

    public class FrameManagerComponent : Component
    {
        private const int StartCapacity = 30000;
        private static bool[] hasFrame = new bool[StartCapacity];
        private static Frame[] frames = new Frame[StartCapacity];
        public static Frame[] Frames { get { return frames; } }
        private static int capacity = StartCapacity;
        private static int _foreSight;
        public static int LoadedFrames { get; private set; }

        public JitterSettings mSetting = new JitterSettings(2f, .1f, .004f);

        public static int ForeSight
        {
            get { return _foreSight; }
            private set
            {
                _foreSight = value;

                //Scaling for latency buffering
            }
        }
        private static bool _adjustFramerate;
        public static bool AdjustFramerate
        {
            get
            {
                return _adjustFramerate;
            }
            set
            {
                _adjustFramerate = value;
            }
        }
        static float jitterFactor = 0f;
        static float lastScaler = 1f;

        public void Awake()
        {

        }

        public void Initialize()
        {
            AdjustFramerate = true;
            EndFrame = -1;

            ForeSight = 0;
            nextFrame = 0;
            System.Array.Clear(hasFrame, 0, hasFrame.Length);
        }

        public void TweakFramerate()
        {
            float jitterCompensation = mSetting.JitterCompensation;
            float jitterSensitivity = mSetting.JitterSensitivity;
            float jitterDegrade = mSetting.JitterDegrade;
            int rate = ETModel.Game.Scene.GetComponent<LockFrameComponent>().FrameRate;
            if (AdjustFramerate)
            {
                float scaler = (float)(ForeSight);

                {
                    if (Mathf.Abs(scaler - lastScaler) > 0)
                        jitterFactor = Mathf.Lerp(jitterFactor, Mathf.Abs(scaler - lastScaler), jitterSensitivity);
                    else
                        jitterFactor = Mathf.Lerp(jitterFactor, 0, jitterDegrade);
                    lastScaler = scaler;

                    float jitterEffect = jitterFactor * jitterCompensation;
                    scaler -= jitterEffect;
                }

                scaler /= rate;
                Time.timeScale = Mathf.Lerp(Time.timeScale, 1 + scaler, 1 / (float)rate);
            }
            else
            {
                //Time.timeScale = 1f;
            }
        }
        private static int nextFrame;

        public static int EndFrame { get; set; }

        public bool CanAdvanceFrame
        {
            get { return (ForeSight > 0 && (ETModel.Game.Scene.GetComponent<LockFrameComponent>().mStartGame)); }
        }
        public static bool HasFrame(int frame)
        {
            return frame < capacity && hasFrame[frame];
        }
        public  void Simulate()
        {

            TweakFramerate();
            ForeSight--;
            /*Frame frame = frames[LockstepManager.InfluenceFrameCount];
            if (frame.Commands.IsNotNull())
            {
                for (int i = 0; i < frame.Commands.Count; i++)
                {
                    Command com = frame.Commands[i];

                    LockstepManager.Execute(com);
                }
            }*/
            //frames[LockstepManager.InfluenceFrameCount] = null;

        }
        public void AddFrame(int frameCount, Frame frame)
        {
            EnsureCapacity(frameCount + 1);
            frames[frameCount] = frame;

            hasFrame[frameCount] = true;

            while (HasFrame(nextFrame))
            {
                ForeSight++;
                nextFrame++;
                LoadedFrames++;
            }
        }
        private void EnsureCapacity(int min)
        {
            if (capacity < min)
            {
                capacity *= 2;
                if (capacity < min)
                {
                    capacity = min;
                }
                Array.Resize(ref frames, capacity);
                Array.Resize(ref hasFrame, capacity);
            }
        }
        public struct JitterSettings
        {
            public JitterSettings(float compensation, float sensitivity, float degrade)
            {
                JitterCompensation = compensation;
                JitterSensitivity = sensitivity;
                JitterDegrade = degrade;
            }
            //Note: Higher jitter factor = more lag bufering
            /// <summary>
            /// The jitter compensation. Higher value = higher effect of jitter factor on lag buffering.
            /// </summary>
            public float JitterCompensation;
            /// <summary> 
            /// The jitter sensitivity. Higher value = bigger effect on jitter factor of each jitter.
            /// </summary>
            public float JitterSensitivity;
            /// <summary>
            /// The jitter degrade. Higher value = faster reduction of jitter factor.
            /// </summary>
            public float JitterDegrade;
        }
    }
}
