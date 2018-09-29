using RollBack;
using RollBack.Input;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ETModel
{
    public class GameState : IGameState
    {
        const int playerCount = 4; // should match number of available input assignments
        readonly Unit[] players = new Unit[playerCount];
        int frame = 0;

        public void AddGameUnit(Unit u)
        {
            players[u.mPlayerIndex] = u;
        }

        public void AfterPrediction()
        {

        }

        public void AfterRollbackAwareFrame()
        {

        }

        public void BeforePrediction()
        {

        }

        public void BeforeRollbackAwareFrame(int frame, bool startupPrediction)
        {

        }

        public void PlayerJoin(int playerIndex, string playerName, byte[] playerData, bool firstTimeSimulated)
        {

        }

        public void PlayerLeave(int playerIndex, bool firstTimeSimulated)
        {
            players[playerIndex] = null;
        }

        public void RollbackDriverDetach()
        {

        }

        public void Update(MultiInputState input, bool firstTimeSimulated)
        {

        }

        #region Serialization
        //快照序列化反序列化
        public byte[] Serialize()
        {
            MemoryStream ms = new MemoryStream();
            BinaryWriter bw = new BinaryWriter(ms);

            return ms.ToArray();
        }

        public void Deserialize(byte[] data)
        {

        }

        #endregion
    }
}
