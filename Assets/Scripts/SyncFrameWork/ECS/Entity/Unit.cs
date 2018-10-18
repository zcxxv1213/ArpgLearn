using RollBack;
using RollBack.Input;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace ETModel
{
	public enum UnitType
	{
		Hero,
		Npc
	}
    public enum Team
    {
        Red,
        Blue
    }
    public sealed class Unit: Entity
	{
        public RollbackDriver mRollebackDriver;
        public int mPlayerIndex;
        public bool ReadyForUpdate = false;
        Dictionary<int, InputState> mFrameWithInputDic = new Dictionary<int, InputState>();

        Queue<C2SCoalesceInput> incomingMessageQueue = new Queue<C2SCoalesceInput>();

        public InputState mNowInpuState = InputState.None;
        public InputAssignment mInputAssignment { get; set; }
        List<KeyCode> allInputList = new List<KeyCode>();
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

        public void QueueMessage(C2SCoalesceInput message)
        {
            incomingMessageQueue.Enqueue(message);
        }

        public C2SCoalesceInput ReadNetMessage()
        {
            if (incomingMessageQueue.Count > 0)
                return incomingMessageQueue.Dequeue();

            return null;
        }

        public void AddInputStateWithFrame(InputState state)
        {
            mNowInpuState = state;
       //     mFrameWithInputDic[mRollebackDriver.CurrentFrame] = state;
        }

        public void UpdateInput(InputState state)
        {
            KeyCode code = InputHelper.GetKeyCodeByInputState(state);
            allInputList.Add(code);
            if (code == KeyCode.UpArrow)
            {
                this.Position = new Vector3(this.Position.x, this.Position.y, this.Position.z + 1);
            }
        }

        public void SetRollBackDriver(RollbackDriver d)
        {
            mRollebackDriver = d;
        }
        public void Serialize(BinaryWriter bw)
        {
            //序列化位置等等信息
            bw.Write(this.mPlayerIndex);
        }

        public byte[] GetNowPlayerData()
        {
            MemoryStream ms = new MemoryStream();
            BinaryWriter bw = new BinaryWriter(ms);
            this.Serialize(bw);
            return ms.ToArray();
        }

        public void DeSerialize(BinaryReader br)
        {
            this.mPlayerIndex = br.ReadInt32();
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