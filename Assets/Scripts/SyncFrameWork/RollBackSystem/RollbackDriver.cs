using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace RollBack
{
    public class RollbackDriver
    {
        readonly IGameState game;
        readonly IAcceptsDesyncDumps dumpTarget;
        readonly int inputBitsUsed;
        public RollbackDriver(IGameState game, IAcceptsDesyncDumps dumpTarget, int inputBitsUsed)
        {
            this.game = game;
            this.dumpTarget = dumpTarget;
            this.inputBitsUsed = inputBitsUsed;
            SetupOnlineStateBuffers();
            SetupInputBuffers();
        }
     
        /// <summary>The <see cref="newestConsistentFrame"/> on the server</summary>
        int serverNewestConsistentFrame;
        /// <summary>Ordered list of join/leave events</summary>
        List<JoinLeaveEvent> joinLeaveEvents = new List<JoinLeaveEvent>();

        /// <summary>The current frame for the game state (how the game appears to the local user).</summary>
        /// <remarks>This is the frame of the last snapshot in the snapshot buffer.</remarks>
        public int CurrentSimulationFrame { get; private set; }

        /// <summary>The current frame for input and network timing (how the game appears to the network).</summary>
        /// <remarks>This is the frame of the last input in the local input buffer.</remarks>
        public int CurrentFrame { get; private set; }
        public const int FramesPerSecond = 60;
        public static readonly TimeSpan FrameTime = new TimeSpan(166667); // 60 FPS

        PacketTimeTracker packetTimeTracker;
        SynchronisedClock synchronisedClock;
        public void ShotDown()
        {
            this.game.RollbackDriverDetach(); 
        }

        #region For Debug Output
        /// <summary>
        /// 本地测试
        /// </summary>
        // This is very quick-and-dirty

        public int LocalNCF { get { return newestConsistentFrame; } }
        public int ServerNCF { get { return serverNewestConsistentFrame; } }

        public int JLEBufferCount { get { return joinLeaveEvents.Count; } }

        public int? InputFirstMissingFrame(int input)
        {
            OnlineState onlineState;
            int onlineStateFrame = onlineStateBuffers[input].TryGetLastBeforeOrAtFrame(CurrentFrame, out onlineState);
            if (onlineStateFrame < 0 || !onlineState.Online)
                return null;

            // Either from after the NCF+1 (because we may have cleared before that) or the connection frame (because there will be no input before that)
            return inputBuffers[input].FirstUnknownFrameFrom(Math.Max(newestConsistentFrame + 1, onlineStateFrame));
        }

        //客户端需要同步时间
        public bool HasClockSyncInfo { get { return true; } }

        public double PacketTimeOffset
        {
            get
            {
                if (HasClockSyncInfo)
                    return packetTimeTracker.DesiredCurrentFrame - CurrentFrame;
                else
                    return 0;
            }
        }

        public double TimerCorrectionRate
        {
            get
            {
                if (HasClockSyncInfo)
                    return synchronisedClock.TimerCorrectionRate;
                else
                    return 1;
            }
        }

        public double SynchronisedClockFrameOffset
        {
            get
            {
                if (HasClockSyncInfo)
                    return synchronisedClock.CurrentFrameContinuious - CurrentFrame;
                else
                    return 1;
            }
        }

        #endregion
        #region Overflow Backstops (constants)

        // See "Rollback Design.pptx" for details
        const int ServerInputMissingBackstop = 20 * FramesPerSecond;
        const int ClientInputMissingBackstop = 40 * FramesPerSecond;
        const int LocalNCFBackstop = 60 * FramesPerSecond;
        const int RemoteNCFBackstop = 90 * FramesPerSecond;
        const int DebugBackstop = 100 * FramesPerSecond; // <- For alerting if we miss a backstop (programmer error)

        const int RemoteJLEBackstop = 50; // <- This is just an arbitrary large number for safety...
        const int RemoteJLEKeepFramesBackstop = RemoteNCFBackstop; // <- ... The bigger concern is buffered JLEs blocking frame clean-up

        const int InputExcessFrontstop = 60 * FramesPerSecond; // <- stop people filling up the input buffer at the front, too

        const int InputBroadcastFloodLimit = ServerInputMissingBackstop; // <- don't send a silly number of input frames to the network all at once

        #endregion

        #region Newest Consistent Frame (NCF)

        /// <summary>
        /// The frame for which we have received all relevant inputs.
        /// Note: On the client, this can be moved backwards as far as <see cref="serverNewestConsistentFrame"/> by the server.
        /// </summary>
        int newestConsistentFrame;
        void UpdateNewestConsistentFrame()
        {

            int inputMissingBackstop = ClientInputMissingBackstop;

            // Advance the NCF frame-by-frame:
            while (true)
            {
                // Check we're not becoming consistent past our own input or simulation state:
                Debug.Assert(newestConsistentFrame <= CurrentFrame);
                Debug.Assert(newestConsistentFrame <= CurrentSimulationFrame);
                if (newestConsistentFrame >= CurrentFrame)
                    goto done;
                if (newestConsistentFrame >= CurrentSimulationFrame)
                    goto done;

                int nextNCF = newestConsistentFrame + 1;

                // Check whether each player has input data for the next NCF:
                for (int p = 0; p < inputBuffers.Length; p++)
                {
                    int onlineStateIndex;
                    OnlineState onlineState;
                    if (onlineStateBuffers[p].TryGetLastBeforeOrAtFrame(nextNCF, out onlineState, out onlineStateIndex) < 0)
                        continue;
                    if (!onlineState.Online)
                        continue;

                    if (!inputBuffers[p].ContainsKey(nextNCF)) // Input not available for next frame
                    {
                        if (nextNCF < CurrentFrame - inputMissingBackstop) // Frame too old
                        {
                            RemotePeer remotePeer = GetRemotePeerForInputIndex(p);
                            if (remotePeer != null && remotePeer.IsConnected) // They're still connected (next time around they won't be - prevents log flood)
                            {
                                if (onlineStateIndex == onlineStateBuffers[p].Count - 1) // They owned the input index at the time of the missing frame
                                {
                                    network.Log("Hit missing input frame backstop for " + remotePeer.PeerInfo + " (input " + p + "), at frame " + nextNCF);
                                    network.NetworkDataError(remotePeer, null);
                                }
                            }
                        }
                        goto done;
                    }
                }

                // Advance the NCF:
                newestConsistentFrame++;
            }
            done:

            // Check if our NCF has gotten stuck!
            if (newestConsistentFrame < CurrentFrame - LocalNCFBackstop)
            {
                // If this happens, it is most likely a programming error causing a buffer desync.
                // However it can happen if the server is misbehaving and not sending us fix-up buffers for leaving clients (in time).
                // (Individual peers getting stuck should be handled above, with a shorter time-out).
                // In theory, this shouldn't happen on the server itself.

                network.Log("Hit local NCF backstop!");
                Debug.Assert(false); // <- because it's probably a programming error

                network.Disconnect(UserVisibleStrings.InputBufferOverflowed);
                throw new NetworkDisconnectionException();
            }


            if (network.IsServer)
            {
                serverNewestConsistentFrame = newestConsistentFrame;
            }

            RunPendingHashChecks();
        }

        #endregion Newest Consistent Frame (NCF)
        /// <summary>For each player (by input index), a sparse frame buffer of online state.</summary>
        /// <remarks>
        /// This doubles as buffer for applying online state changes to the game state.
        /// For implementation simplicity, each input assignment is considered in order (rather than running in event order).
        /// (This is ok, as this buffer is used for rollback anyway.)
        /// </remarks>
        FrameDataBuffer<OnlineState>[] onlineStateBuffers;

        void SetupOnlineStateBuffers()
        {
            onlineStateBuffers = new FrameDataBuffer<OnlineState>[InputAssignmentExtensions.MaxPlayerInputAssignments];
            for (int i = 0; i < onlineStateBuffers.Length; i++)
            {
                onlineStateBuffers[i] = new FrameDataBuffer<OnlineState>();
            }
        }
        #region Online State Buffer

        private class OnlineState
        {
            /// <param name="joiningPlayerName">The name of a joining player, or null for a leaving player</param>
            public OnlineState(int eventId, string joiningPlayerName, byte[] joiningPlayerData, OnlineState previousThisFrame = null)
            {
                this.EventId = eventId;
                this.JoiningPlayerName = joiningPlayerName;
                this.PreviousThisFrame = previousThisFrame;
                this.JoiningPlayerData = joiningPlayerData;

                Debug.Assert(previousThisFrame == null || previousThisFrame.Online != this.Online); // Ordering (callers are expected to check/enforce this)
            }

            public int EventId { get; private set; }

            public string JoiningPlayerName { get; private set; }
            public byte[] JoiningPlayerData { get; private set; }

            /// <summary>The previous JLE for the given frame and input assignment</summary>
            public OnlineState PreviousThisFrame { get; private set; }

            public bool Online { get { return JoiningPlayerName != null; } }
        }

        #endregion

        InputBuffer[] inputBuffers;

        void SetupInputBuffers()
        {
            inputBuffers = new InputBuffer[InputAssignmentExtensions.MaxPlayerInputAssignments];
            for (int i = 0; i < inputBuffers.Length; i++)
            {
                inputBuffers[i] = new InputBuffer();
            }
        }

        #region Join/Leave Events

        private struct JoinLeaveEvent
        {
            /// <param name="joiningPlayerName">The name of a joining player, or null for a leaving player</param>
            public JoinLeaveEvent(int eventId, int consistentFrame, int frame, int inputIndex, string joiningPlayerName, byte[] joiningPlayerData)
            {
                this.eventId = eventId;
                this.consistentFrame = consistentFrame;
                this.frame = frame;
                this.inputIndex = inputIndex;
                this.joiningPlayerName = joiningPlayerName;
                this.joiningPlayerData = joiningPlayerData;

                Debug.Assert(eventId != 0);
                Debug.Assert(frame > consistentFrame); // Can only make modifications after the consistency point
                Debug.Assert(joiningPlayerName != null || joiningPlayerData == null); // Only have player data for joins
            }

            public readonly int eventId;
            public readonly int consistentFrame;
            public readonly int frame;
            public readonly int inputIndex;

            public readonly string joiningPlayerName;
            public readonly byte[] joiningPlayerData;

            public bool Join { get { return joiningPlayerName != null; } }
            public bool Leave { get { return joiningPlayerName == null; } }


            const int inputIndexBits = 7;
            //先写死
            /// <summary>Read from network with an already-known event ID</summary>
            public JoinLeaveEvent(int eventId)
            {
                this.eventId = eventId;
                this.consistentFrame = 1;
                this.frame = consistentFrame + 1;
                if (frame <= consistentFrame)
                    Debug.LogError("Join/Leave Event frame number out of range");
                Debug.Assert(InputAssignmentExtensions.MaxPlayerInputAssignments < (1 << inputIndexBits));
                this.inputIndex = 1;
                if (inputIndex >= InputAssignmentExtensions.MaxPlayerInputAssignments)
                    Debug.LogError("Join/Leave Event input index out of range");

                if (true)
                {
                    this.joiningPlayerName = "Name";
                    this.joiningPlayerData = new byte[0];
                }
                else
                {
                    this.joiningPlayerName = null;
                    this.joiningPlayerData = null;
                }
            }

            /*
            public void WriteToNetworkNoEventId(NetOutgoingMessage message)
            {
                Debug.Assert(frame > consistentFrame);
                message.Write(consistentFrame);
                message.WriteVariableUInt32((uint)(frame - consistentFrame));
                Debug.Assert(InputAssignmentExtensions.MaxPlayerInputAssignments < (1 << inputIndexBits));
                message.Write((byte)inputIndex, inputIndexBits);
                message.Write(joiningPlayerName != null);
                if (joiningPlayerName != null)
                {
                    message.Write(joiningPlayerName);
                    message.WriteByteArray(joiningPlayerData);
                }
            }*/

        }
        #endregion Join/Leave Events
    }
}
