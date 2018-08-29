using RollBack.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RollBack
{
    public interface IGameState
    {
        /// <param name="startupPrediction">Frame is being executed during network startup, to catch up the syncronised clock time</param>
        void BeforeRollbackAwareFrame(int frame, bool startupPrediction);

        void AfterRollbackAwareFrame();

        void RollbackDriverDetach();



        /// <param name="firstTimeSimulated">This is the first time this frame has been simulated (play sound effects, advance smoothing, etc)</param>
        /// <param name="displayToUser">This update is expected to be displayed to the user</param>
        void Update(MultiInputState input, bool firstTimeSimulated);


        // TODO: Add "player grouping bits" to this method (to support "multiple players per network host")
        //       (Right now we are faking it in RCRU's GameStateManager.)
        void PlayerJoin(int playerIndex, string playerName, byte[] playerData, bool firstTimeSimulated);
        void PlayerLeave(int playerIndex, bool firstTimeSimulated);


        /// <summary>Use to capture data for smoothing</summary>
        void BeforePrediction();
        /// <summary>Use to apply captured smoothing data</summary>
        void AfterPrediction();
    }
}
