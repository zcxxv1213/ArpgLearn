using RollBack;
using RollBack.Input;
using System.Collections.Generic;
using UnityEngine;

namespace ETModel
{
	public enum UnitType
	{
		Hero,
		Npc
	}
	
	public sealed class Unit: Entity
	{
        public RollbackDriver mRollebackDriver;
        public int mPlayerIndex;
        public bool ReadyForUpdate = false;
        Dictionary<int, InputState> mFrameWithInputDic = new Dictionary<int, InputState>();

        Queue<C2SInputMessage> incomingMessageQueue = new Queue<C2SInputMessage>();

        public InputState mNowInpuState = InputState.None;
        public InputAssignment mInputAssignment { get; set; }
        public VInt3 IntPos;
        private string mName;
		public GameObject GameObject;
        public Vector3 Pos;
        public long mPlayerID { get; set; }
        public void Awake()
		{
		}
        public void SetPlayerInputIndex(int i)
        {
            mPlayerIndex = i;
        }
        public string name
        {
            get
            {
                return mName;
            }
            set
            {
                mName = value;
            }
        }

        public Vector3 Position
		{
			get
			{
				return Pos;
			}
			set
			{
                Pos = value;
			}
		}

		public Quaternion Rotation
		{
			get
			{
				return GameObject.transform.rotation;
			}
			set
			{
				GameObject.transform.rotation = value;
			}
		}

        public void QueueMessage(C2SInputMessage message)
        {
            incomingMessageQueue.Enqueue(message);
        }

        public C2SInputMessage ReadNetMessage()
        {
            if (incomingMessageQueue.Count > 0)
                return incomingMessageQueue.Dequeue();

            return null;
        }

        public void AddInputStateWithFrame(InputState state)
        {
            mNowInpuState = state;
            mFrameWithInputDic[mRollebackDriver.CurrentFrame] = state;
        }

        public void SetRollBackDriver(RollbackDriver d)
        {
            mRollebackDriver = d;
        }

        public override void Dispose()
		{
			if (this.IsDisposed)
			{
				return;
			}

			base.Dispose();
		}
	}
}