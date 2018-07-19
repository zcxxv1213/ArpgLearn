using Com.Game.ObjectPool;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Com.Game.Utils.Timers
{
    class GameTimer
    {
        private GameTimerManager mGameTimerManager = GameTimerManager.Instance;

        public Action mTimerHandler;
        public Action mTimerCompleteHandler;

        public bool mStopOnChangeScene = false;
        public bool mIgnoreOnPause { get; private set; }

        public int mCurrentCount { get; private set; }
        public float mDelay { get; set; }
        public float mRepeatCount { get; set; }
        public bool mInPool { get; set; }
        public bool mRunning { get; private set; }

        internal double mNextExecuteTime = 0;
        internal double mStartTime = 0;
        internal float mDuration = 0;

        internal LinkedListNode<GameTimer> mSelfNode = null;
        internal LinkedList<GameTimer> mSelfList = null;

        
        public static GameTimer SetTimeout(float delay, Action completeHandler, bool stopOnChangeScene = false, bool ignoreOnPause = false)
        {
            GameTimer timer = GameTimer.GetGameTimer();
            timer.Init(delay, 1, null, completeHandler, stopOnChangeScene, ignoreOnPause);
            timer.Start();
            return timer;
        }

        public static GameTimer SetInterval(float delay, Action timerHandler, bool stopOnChangeScene = false, bool ignoreOnPause = false)
        {
            GameTimer timer = GameTimer.GetGameTimer();
            timer.Init(delay, 0, timerHandler, null, stopOnChangeScene, ignoreOnPause);
            timer.Start();
            return timer;
        }

        //每帧执行，一共执行多久,不允许暂停
        public static GameTimer SetFrameExecute(float duration, Action timerHandler, Action timerCompleteHandler = null, bool stopOnChangeScene = false)
        {
            GameTimer timer = GameTimer.GetGameTimer();
            timer.Init(0, 0, timerHandler, timerCompleteHandler, stopOnChangeScene);
            timer.mDuration = duration;
            timer.Start();
            return timer;
        }

        //一共执行多少帧
        public static GameTimer ExecuteTotalFrames(int framecount, Action timerHandler, Action timerCompleteHandler = null, bool stopOnChangeScene = false, bool ignoreOnPause = false)
        {
            GameTimer timer = GameTimer.GetGameTimer();
            timer.Init(0, framecount, timerHandler, timerCompleteHandler, stopOnChangeScene, ignoreOnPause);
            timer.Start();
            return timer;
        }

        public static GameTimer GetGameTimer()
        {
            return ObjectPoolManager.mGameTimerPool.Get();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="delay">计时器事件间的延迟（以秒为单位）</param>
        /// <param name="repeatCount">指定重复次数。如果为零，则计时器将持续不断重复运行；如果不为 0，则将运行计时器，运行次数为指定的次数，然后停止</param>
        /// <param name="timerHandler"></param>
        /// <param name="timerCompleteHandler"></param>
        public void Init(float delay, int repeatCount = 0, Action timerHandler = null, Action timerCompleteHandler = null, bool stopOnChangeScene = false, bool ignoreOnPause = false)
        {
            this.mDelay = delay;
            this.mRepeatCount = repeatCount;
            this.mTimerHandler = timerHandler;
            this.mTimerCompleteHandler = timerCompleteHandler;
            this.mStopOnChangeScene = stopOnChangeScene;
            this.mIgnoreOnPause = ignoreOnPause;
            this.mInPool = false;
            Reset();
        }

        public void Reset()
        {
            CheckInPool();

            this.mCurrentCount = 0;

            Stop();
        }

        public void ReStart()
        {
            CheckInPool();

            Reset();

            Start();
        }

        public void Start()
        {
            if (mRunning == false)
            {
                CheckInPool();
                mGameTimerManager.AddGameTimer(this);
                mRunning = true;
            }
        }

        public void Stop()
        {
            if (mRunning == true)
            {
                CheckInPool();

                mStartTime = 0;

                mGameTimerManager.RemoveGameTimer(this);
                mRunning = false;
            }
        }

        internal void Execute()
        {
            ++mCurrentCount;

            if (mTimerHandler != null)
                mTimerHandler();

            if (mRepeatCount != 0 && mCurrentCount >= mRepeatCount)
            {
                mRunning = false;

                if (mTimerCompleteHandler != null)
                    mTimerCompleteHandler();
            }
            else if (mDuration > 0)
            {
                double curTime = mIgnoreOnPause ? mGameTimerManager.mScaleTimeSinceStartup : mGameTimerManager.mRealTimeSinceStartup;

                if (mStartTime == 0)
                    mStartTime = curTime;

                if (mStartTime + mDuration > curTime)
                    return;

                mRunning = false;

                if (mTimerCompleteHandler != null)
                    mTimerCompleteHandler();
            }
        }

        public void ClearAction()
        {
            mTimerHandler = null;
            mTimerCompleteHandler = null;
        }

        public void Dispose()
        {
            if (this.mInPool == false)
            {
                Stop();
                ClearAction();
                this.mInPool = true;
                ObjectPoolManager.mGameTimerPool.Put(this);
            }
           
        }
        public void CheckInPool()
        {
            if (this.mInPool)
                Debug.LogError("GameTimer Error");
        }
    }
}
