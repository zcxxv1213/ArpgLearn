using RollBack;
using RollBack.Input;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

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
            Debug.Log("After");
        }

        public void BeforePrediction()
        {

        }

        public void BeforeRollbackAwareFrame(int frame, bool startupPrediction)
        {

        }

        public void PlayerJoin(int playerIndex, string playerName, byte[] playerData, bool firstTimeSimulated)
        {
            Debug.Log("PlayerJoin" + "," + playerIndex + "," + playerName + "," + firstTimeSimulated);
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
            frame++;
            Debug.Log(frame);
            Debug.Log(players.Length);
            foreach (var v in players)
            {
                if (v != null)
                {
                    InputState state = input[v.mInputAssignment.GetFirstAssignedPlayerIndex()];
                    Debug.Log("InpuState: " + state + "   " + "PlayerID" + v.mPlayerID + "CurrentFrame" + frame);
                    v.UpdateInput(state);
                }
            }
            Debug.Log("AfterUpdate");
        }

        #region Serialization
        //快照序列化反序列化
        public byte[] Serialize()
        {
            MemoryStream ms = new MemoryStream();
            BinaryWriter bw = new BinaryWriter(ms);
            bw.Write(frame);
            Debug.Log("序列化 Frame " + frame);
            for (int i = 0; i < players.Length; i++)
            {
                if (bw.WriteBoolean(players[i] != null))
                {
                    players[i].Serialize(bw);
                }
            }
            return ms.ToArray();
        }

        public void Deserialize(byte[] data)
        {
            Debug.Log(data.Length);
            MemoryStream ms = new MemoryStream(data);
            BinaryReader br = new BinaryReader(ms);

            frame = br.ReadInt32();
            Debug.Log("反序列化 Frame " + frame + ","+ players.Length);
            for (int i = 0; i < players.Length; i++)
            {
                if (br.ReadBoolean())
                {
                    Debug.Log("True");
                    //队伍信息先写死
                    Unit u = ComponentFactory.Create<Unit, UnitType, Team>(UnitType.Hero, Team.Blue);
                    u.DeSerialize(br);
                    this.AddGameUnit(u);
                }
                else
                {
                    Debug.Log("False");
                    players[i] = null;
                }
            }
        }

        #endregion
    }
}
