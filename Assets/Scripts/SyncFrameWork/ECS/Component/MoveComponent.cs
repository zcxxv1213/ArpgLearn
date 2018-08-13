using System.Collections.Generic;
using UnityEngine;

namespace ETModel
{
	[ObjectSystem]
	public class MoveComponentAwakeSystem : AwakeSystem<MoveComponent>
	{
		public override void Awake(MoveComponent self)
		{
			self.Awake();
		}
	}

	[ObjectSystem]
	public class MoveComponentUpdateSystem : UpdateSystem<MoveComponent>
	{
		public override void Update(MoveComponent self)
		{
			self.Update();
		}
	}

	public class Speed
	{
		public long Id;

		public Vector3 Value;

		public Speed()
		{
		}

		public Speed(long id)
		{
			this.Id = id;
		}
	}

	public class MoveComponent : Component
	{
		public long mainSpeed;
		public Dictionary<long, Speed> speeds = new Dictionary<long, Speed>();

		// turn
		public Quaternion To;
		public Quaternion From;
		public float t = float.MaxValue;
		public float TurnTime = 0.1f;

		public bool IsArrived { get; private set; } = true;


		public bool hasDest;
		public Vector3 Dest;

		public Vector3 MainSpeed
		{
			get
			{
				Speed speed;
				if (!this.speeds.TryGetValue(this.mainSpeed, out speed))
				{
					speed = new Speed(this.mainSpeed);
					this.speeds.Add(speed.Id, speed);
				}
				return speed.Value;
			}
			set
			{
				Speed speed;
				if (!this.speeds.TryGetValue(this.mainSpeed, out speed))
				{
					speed = new Speed(this.mainSpeed);
					this.speeds.Add(speed.Id, speed);
				}
				speed.Value = value;
			}
		}

		public Vector3 Speed
		{
			get
			{
				Vector3 speed = new Vector3();
				foreach (Speed sp in this.speeds.Values)
				{
					speed += sp.Value;
				}
				return speed;
			}
		}

		public void Awake()
		{
			this.mainSpeed = this.AddSpeed(new Vector3());
		}

		public void Update()
		{
			UpdateTurn();

			if (this.IsArrived)
			{
				return;
			}

			if (this.Speed == Vector3.zero)
			{
				return;
			}

			Unit unit = this.GetParent<Unit>();
			Vector3 moveVector3 = this.Speed * Time.deltaTime;

			if (this.hasDest)
			{
				float dist = (this.Dest - unit.Position).magnitude;
				if (moveVector3.magnitude >= dist || dist < 0.1f)
				{
					unit.Position = this.Dest;
					this.IsArrived = true;
					return;
				}
			}

			unit.Position = unit.Position + moveVector3;
		}

		private void UpdateTurn()
		{
			//Log.Debug($"update turn: {this.t} {this.TurnTime}");
			if (this.t > this.TurnTime)
			{
				return;
			}

			this.t += Time.deltaTime;

			Quaternion v = Quaternion.Slerp(this.From, this.To, this.t / this.TurnTime);
			this.GetParent<Unit>().Rotation = v;
		}

		public void MoveToDest(Vector3 dest, float speedValue)
		{
			if ((dest - this.GetParent<Unit>().Position).magnitude < 0.1f)
			{
				this.IsArrived = true;
				return;
			}
			this.IsArrived = false;
			this.hasDest = true;
			Vector3 speed = dest - this.GetParent<Unit>().Position;
			speed = speed.normalized * speedValue;
			this.MainSpeed = speed;
			this.Dest = dest;
		}

		public void MoveToDir(Vector3 dir)
		{
			this.IsArrived = false;
			this.hasDest = false;
			this.MainSpeed = dir;
		}

		public long AddSpeed(Vector3 spd)
		{
			Speed speed = new Speed() { Value = spd };
			this.speeds.Add(speed.Id, speed);
			return speed.Id;
		}

		public Speed GetSpeed(long id)
		{
			Speed speed;
			this.speeds.TryGetValue(id, out speed);
			return speed;
		}

		public void RemoveSpeed(long id)
		{
			Speed speed;
			if (!this.speeds.TryGetValue(id, out speed))
			{
				return;
			}
			this.speeds.Remove(id);
		}

		/// <summary>
		/// 停止移动Unit,只停止地面正常移动,不停止击飞等移动
		/// </summary>
		public void Stop()
		{
			this.speeds.Clear();
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