using RollBack;
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
        public int mPlayerIndex;
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