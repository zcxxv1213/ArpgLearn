using RollBack;
using System;
using System.Collections.Generic;
using System.Text;

namespace ETModel
{
    [ObjectSystem]
    public class WorldEntityAwakeSystem : AwakeSystem<WorldEntity>
    {
        public override void Awake(WorldEntity self)
        {
            self.Awake();
        }
    }
    public class WorldEntity : Entity
    {
        GameState mGameState = null;
        public List<Unit> mUnitList = new List<Unit>();
        Dictionary<int, Unit> mInputIndexDic = new Dictionary<int, Unit>();
        public int maxInputCount = 4;
        private Unit mMainUnit;
        public void Awake()
        {
            this.AddComponent<WorldManagerComponent>();
            this.AddComponent<RollbackDriver>();
            this.GetComponent<RollbackDriver>().SetWorldEntity(this);
        }

        public void SetMainUnit(Unit u)
        {
            mMainUnit = u;
        }

        public Unit GetMainUnit()
        {
            return mMainUnit;
        }

        public void SetGameState(GameState g)
        {
            mGameState = g;
            //TODO 修改
            this.GetComponent<RollbackDriver>().InitialDriver(g, null, 4);
        }
        public void BroadCastMessageToUnitsInWorld(IActorMessage message)
        {

        }
        public void AddUnit(Unit u)
        {
            if (!mUnitList.Contains(u))
            {
                mUnitList.Add(u);
                this.AddUnitWithInputIndex(u);
                u.SetRollBackDriver(this.GetComponent<RollbackDriver>());
            }
            else
            {
                Log.Warning("重复添加UnitTo World");
            }
        }

        private void AddUnitWithInputIndex(Unit mUnit)
        {
            Unit U;
            for (int i = 1; i < maxInputCount; i++)
            {
                if (mInputIndexDic.TryGetValue(i, out U) == false)
                {
                    mInputIndexDic[i] = mUnit;
                    mUnit.SetPlayerInputIndex(i);
                    mUnit.mInputAssignment = AssignInput();
                    Log.Info("InpuAssignmet" + mUnit.mInputAssignment);
                    return;
                }
            }
        }

        #region Input Assignment

        InputAssignment assignedInputs;

        bool IsFull { get { return assignedInputs == InputAssignment.Full; } }

        InputAssignment AssignInput()
        {
            InputAssignment next = assignedInputs.GetNextAssignment();
            if (next == 0)
                Log.Error("没有下一个InputAssign");
            assignedInputs |= next;
            return next;
        }
        //TODO RELEASE
        void ReleaseInputAssignment(InputAssignment assignment)
        {
            assignedInputs &= ~assignment;
        }

        #endregion

        public bool ReadyForUpdate()
        {
            if (mUnitList.Count > 0)
            {
                foreach (var v in mUnitList)
                {
                    if (v.ReadyForUpdate == false)
                        return false;
                }
                return true;
            }
            return false;
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
