using RollBack.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using ETModel;

namespace RollBack
{
    public class RollbackDriver:ETModel.Component
    {
        readonly IGameState game;
        readonly IAcceptsDesyncDumps dumpTarget;
        readonly int inputBitsUsed;
        ETModel.Component mNetCompoonent = ETModel.Game.Scene.GetComponent<NetworkComponent>();

        public RollbackDriver(IGameState game, IAcceptsDesyncDumps dumpTarget, int inputBitsUsed)
        {
            this.game = game;
            this.dumpTarget = dumpTarget;
            this.inputBitsUsed = inputBitsUsed;
            SetupOnlineStateBuffers();
            SetupInputBuffers();
        }



        /// <summary>The current (last received) JLE. Used to syncronise NCF updates between client and server when clients join/leave. 0 if no events have been received yet.</summary>
        int latestJoinLeaveEvent;

        /// <summary>The connection ID for each JLE, indexed by JLE# (not by frame)</summary>
        FrameDataBuffer<int> hostForJLE = new FrameDataBuffer<int>();

        int GetCurrentHostId()
        {
            Debug.Assert(hostForJLE.Count > 0);
            return hostForJLE.Values[hostForJLE.Count - 1];
        }


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
                          /*  RemotePeer remotePeer = GetRemotePeerForInputIndex(p);
                            if (remotePeer != null && remotePeer.IsConnected) // They're still connected (next time around they won't be - prevents log flood)
                            {
                                if (onlineStateIndex == onlineStateBuffers[p].Count - 1) // They owned the input index at the time of the missing frame
                                {
                                    network.Log("Hit missing input frame backstop for " + remotePeer.PeerInfo + " (input " + p + "), at frame " + nextNCF);
                                    network.NetworkDataError(remotePeer, null);
                                }
                            }*/
                            //TODO 检测帧，如果太旧那么踢出客户端（服务端做）
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

                //TODO最新的帧炸裂了发送给服务器炸裂信息并且踢出
          /*      network.Log("Hit local NCF backstop!");
                Debug.Assert(false); // <- because it's probably a programming error

                network.Disconnect(UserVisibleStrings.InputBufferOverflowed);
                throw new NetworkDisconnectionException();*/
            }


           /* if (network.IsServer)
            {
                serverNewestConsistentFrame = newestConsistentFrame;
            }*/

            RunPendingHashChecks();
        }
        void ResetLocalNCF(int frame)
        {
         //   Debug.Assert(!network.IsServer);
            Debug.Assert(frame >= CleanUpBeforeFrame);

            if (newestConsistentFrame > frame)
            {
                newestConsistentFrame = frame;

                // Hashes are no longer valid
                hashBuffer.RemoveAllFrom(frame + 1);
            }
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

        /// <summary>Append an online state change for a particular input index. Changes must be applied in order per-input-index.</summary>
        /// <param name="joiningPlayerName">The name of a joining player, or null for a leaving player</param>
        void AppendOnlineStateChange(int eventId, int inputIndex, int frame, string joiningPlayerName, byte[] joiningPlayerData)
        {
            FrameDataBuffer<OnlineState> onlineStateBuffer = onlineStateBuffers[inputIndex];

            // Each input index's Join/Leave events must be ordered by frame (ie: cannot insert a join before a leave,
            // which would represent two clients online at the same time, which is not possible)
            if (onlineStateBuffer.Count > 0)
            {
                int lastOnlineStateFrame = onlineStateBuffer.Keys[onlineStateBuffer.Count - 1];
                OnlineState lastOnlineState = onlineStateBuffer.Values[onlineStateBuffer.Count - 1];

                if (frame < lastOnlineStateFrame || (joiningPlayerName != null) == lastOnlineState.Online)
                {
                    //  Debug.Assert(!network.IsServer); // <- If this ever happens on the server it's a local programming error!
                    Debug.LogError("Bad online state ordering");
                }
            }

            if (onlineStateBuffer.Count > 0 && onlineStateBuffer.Keys[onlineStateBuffer.Count - 1] == frame)
            {
                // Already had an event on that frame, so "stack" a new one on top
                onlineStateBuffer[onlineStateBuffer.Keys[onlineStateBuffer.Count - 1]] = new OnlineState(eventId, joiningPlayerName, joiningPlayerData, onlineStateBuffer.Values[onlineStateBuffer.Count - 1]);
            }
            else
            {
                onlineStateBuffer.Add(frame, new OnlineState(eventId, joiningPlayerName, joiningPlayerData));
            }
        }



        void RunGameJoinLeaveEventsRecursiveHelper(int frame, int inputIndex, OnlineState onlineState, bool firstTimeSimulated)
        {
            // Recurse to start of linked list before running events in order
            if (onlineState.PreviousThisFrame != null)
                RunGameJoinLeaveEventsRecursiveHelper(frame, inputIndex, onlineState.PreviousThisFrame, firstTimeSimulated);

            if (onlineState.Online)
                game.PlayerJoin(inputIndex, onlineState.JoiningPlayerName, onlineState.JoiningPlayerData, firstTimeSimulated);
            else
                game.PlayerLeave(inputIndex, firstTimeSimulated);
        }

        void RunGameJoinLeaveEvents(int frame, bool firstTimeSimulated)
        {
            for (int i = 0; i < onlineStateBuffers.Length; i++)
            {
                OnlineState onlineState;
                if (onlineStateBuffers[i].TryGetValue(frame, out onlineState))
                {
                    RunGameJoinLeaveEventsRecursiveHelper(frame, i, onlineState, firstTimeSimulated);
                }
            }
        }



        /// <summary>
        /// Returns the frame number of the join event associated with a given leave event, or a later frame up to or equal to the leave frame.
        /// </summary>
        /// <remarks>
        /// Note that there is no sync between JLE clean-up and online-state clean-up, so it's possible that the join state change
        /// (and even the leave state change) may not be found. Even if it is found, it may be ahead of the true join frame, as
        /// a client coming online will set online-state at the consistent frame.
        /// Because we can return a later-than-true frame normally, just return the latest possible frame if we can't find either frame.
        /// </remarks>
        int FindMinimumKnownJoinFrameForLeaveEvent(int eventId, int inputIndex, int leaveFrame)
        {
            var onlineStateBuffer = onlineStateBuffers[inputIndex];

            OnlineState onlineStateForLeave;
            if (!onlineStateBuffer.TryGetValue(leaveFrame, out onlineStateForLeave))
            {
                return leaveFrame; // not found
            }

            // Walk backwards to find the actual leave state change:
            while (onlineStateForLeave.EventId != leaveFrame)
            {
                if (onlineStateForLeave.PreviousThisFrame == null)
                    return leaveFrame; // not found
                onlineStateForLeave = onlineStateForLeave.PreviousThisFrame;
            }

            if (onlineStateForLeave.PreviousThisFrame != null)
            {
                Debug.Assert(onlineStateForLeave.Online == false);
                Debug.Assert(onlineStateForLeave.PreviousThisFrame.Online == true);
                return leaveFrame; // Left on the same frame as the join (was online for 0 frames)
            }

            // There are no more online state changes on the leaving frame.
            // So the last online state change before the leave frame must be the expected join:

            OnlineState onlineStateForJoin;
            int joinFrame = onlineStateBuffer.TryGetLastBeforeOrAtFrame(leaveFrame - 1, out onlineStateForJoin);

            if (joinFrame == -1)
                return leaveFrame; // not found

            Debug.Assert(onlineStateForJoin.Online);
            return joinFrame;
        }


        int FindLocalJoinFrame()
        {
          //  Debug.Assert(network.IsApplicationConnected);

            // We should be the latest entry in our own online state buffer:
         //   var onlineStateBuffer = onlineStateBuffers[network.LocalPeerInfo.InputAssignment.GetFirstAssignedPlayerIndex()];
            var onlineStateBuffer = onlineStateBuffers[0];
            Debug.Assert(onlineStateBuffer.Values[onlineStateBuffer.Count - 1].Online);
       //     Debug.Assert(onlineStateBuffer.Values[onlineStateBuffer.Count - 1].JoiningPlayerName == network.LocalPeerInfo.PlayerName);

            return onlineStateBuffer.Keys[onlineStateBuffer.Count - 1];
        }

        #endregion


        #region Snapshot Buffer

        // TODO: Make the snapshot buffer a sparse buffer to save memory and serialization time
        //       For example: don't need to snapshot any frames that will never be the earliest
        //       input mismatch frame (eg: if we have all inputs for that frame).
        //       Peers that are timing out don't really need to generate all 25 (or so) seconds of snapshots.
        //       Is there a "time to serialize" vs "time to simulate from an earlier frame" trade-off to make?

        // TODO: Pool the objects used as snapshots and to generate snapshots
        //对象池优化，不需要的组件快照优化
        FrameDataBuffer<byte[]> snapshotBuffer = new FrameDataBuffer<byte[]>();

        #endregion
        #region Desync Detection


        /// <summary>Lazy-calculated hashes from the snapshot buffer of frames that are fully consistent (before or at the NCF)</summary>
        readonly FrameDataBuffer<uint> hashBuffer = new FrameDataBuffer<uint>();

        private uint GetHashForSnapshot(int frame)
        {
            // NOTE: Not asserting against CleanUpBeforeFrame, because the way we get called is after NCF gets moved,
            //       but *before* cleanup actually happens. We just check the snapshot exists instead.
            Debug.Assert(frame <= newestConsistentFrame);
            Debug.Assert(snapshotBuffer.ContainsKey(frame));

            uint result;
            if (hashBuffer.TryGetValue(frame, out result))
                return result;

            result = FastHash.Hash(snapshotBuffer[frame]);
            hashBuffer.Add(frame, result);
            return result;
        }



        void ReceiveRemoteHash(int remoteNCF, uint remoteHash, int remoteJLE, int remoteHost, int connectionId)
        {
            if (remoteNCF <= newestConsistentFrame)
                DoHashCheck(remoteNCF, remoteHash, remoteJLE, remoteHost, connectionId);
            else
                AddPendingHashCheck(remoteNCF, remoteHash, remoteJLE, remoteHost, connectionId);
        }

        void DoHashCheck(int remoteNCF, uint remoteHash, int remoteJLE, int remoteHost, int connectionId)
        {
            Debug.Assert(remoteNCF <= newestConsistentFrame);

            if (remoteJLE != latestJoinLeaveEvent || remoteHost != GetCurrentHostId())
                return; // This hash is associated with a different branch of the game state

            uint localHash = GetHashForSnapshot(remoteNCF);

            if (localHash != remoteHash)
            {
                // Log and transfer desync dump
                HandleDesync(remoteNCF, remoteHash, remoteJLE, remoteHost, connectionId);
            }
        }


        // TODO: Refactor: consider passing the PendingHashCheck bundle around instead of loose arguments
        //重构结构
        struct PendingHashCheck
        {
            public uint hash;
            public int remoteJLE, remoteHost;
            public int connectionId;
        }

        // SoA
        readonly List<int> pendingHashCheckFrames = new List<int>();
        readonly List<PendingHashCheck> pendingHashChecks = new List<PendingHashCheck>();


        void AddPendingHashCheck(int remoteNCF, uint remoteHash, int remoteJLE, int remoteHost, int connectionId)
        {
            pendingHashCheckFrames.Add(remoteNCF);
            pendingHashChecks.Add(new PendingHashCheck { hash = remoteHash, remoteJLE = remoteJLE, remoteHost = remoteHost, connectionId = connectionId });
        }

        void RunPendingHashChecks()
        {
            Debug.Assert(pendingHashCheckFrames.Count == pendingHashChecks.Count);

            for (int i = pendingHashCheckFrames.Count - 1; i >= 0; i--)
            {
                if (pendingHashCheckFrames[i] <= newestConsistentFrame)
                {
                    DoHashCheck(pendingHashCheckFrames[i],
                            pendingHashChecks[i].hash, pendingHashChecks[i].remoteJLE, pendingHashChecks[i].remoteHost, pendingHashChecks[i].connectionId);

                    // Unordered removal:
                    int last = pendingHashCheckFrames.Count - 1;
                    pendingHashCheckFrames[i] = pendingHashCheckFrames[last];
                    pendingHashCheckFrames.RemoveAt(last);
                    pendingHashChecks[i] = pendingHashChecks[last];
                    pendingHashChecks.RemoveAt(last);
                }
            }
        }

        #endregion

        #region Join/Leave Events
        /// <summary>Ordered list of join/leave events</summary>
        List<JoinLeaveEvent> joinLeaveEvents = new List<JoinLeaveEvent>();
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

        #region Desync Dump

        const int desyncDumpChannel = 1;

        // Network safety:
        const int desyncMaxDumpFrames = 60;
        const int desyncMaxSnapshotSize = 8192;
        const int desyncMaxPayloadSize = 256 * 1024; // <- Massive amount of data.


        //
        // LOCAL / OUTGOING:
        //


        // Dual usage: Track the frame of a desync we are pending to match for a dump, or int.MaxValue if we have handled or
        //             given up on ever receiving that dump (for the purposes of clean-up); AND: existance of a given key tracks
        //             whether we have sent our own dump to them.
        Dictionary<int, int> desyncedRemotes = new Dictionary<int, int>();

        int GetDesyncCleanupPoint()
        {
            int oldest = int.MaxValue;
            foreach (var frame in desyncedRemotes.Values)
            {
                if (frame < oldest)
                    if (frame < newestConsistentFrame - (desyncMaxDumpFrames + 60)) // <- plenty of time to receieve a desync dump
                        oldest = frame;
            }

            return oldest;
        }


        void HandleDesync(int remoteNCF, uint remoteHash, int remoteJLE, int remoteHost, int connectionId)
        {
            // Try to reconstruct who was responsible:
            /*RemotePeer remotePeer = null;
            foreach (var rp in network.RemotePeers)
            {
                if (rp.PeerInfo.ConnectionId == connectionId)
                {
                    remotePeer = rp;
                    break;
                }
            }*/
            Debug.Log("DESYNC! Peer #" + connectionId + " (no longer connected) desynced at frame " + remoteNCF + " with JLE " + remoteJLE);
           /* if (remotePeer == null)
            {
                network.Log("DESYNC! Peer #" + connectionId + " (no longer connected) desynced at frame " + remoteNCF + " with JLE " + remoteJLE);
                return;
            }*/

            if (!desyncedRemotes.ContainsKey(connectionId)) // <- first desync encountered
            {
                desyncedRemotes.Add(connectionId, remoteNCF);
                Debug.Log("DESYNC! Peer #" + connectionId  + "\" desynced at frame " + remoteNCF + " with JLE " + remoteJLE);
              //  network.Log("DESYNC! Peer #" + connectionId + " \"" + remotePeer.PeerInfo.PlayerName + "\" desynced at frame " + remoteNCF + " with JLE " + remoteJLE);


                // Try to send as many frames back as we can, up to the limit...
                int count = 0;
                int packetSize = 0;
                while (count < desyncMaxDumpFrames && snapshotBuffer.ContainsKey(remoteNCF - count))
                {
                    var snapshotSize = snapshotBuffer[remoteNCF - count].Length;
                    if (snapshotSize > desyncMaxSnapshotSize)
                        break;
                    if (packetSize + snapshotSize > desyncMaxPayloadSize)
                        break;

                    packetSize += snapshotSize;
                    count++;
                }
                Debug.Assert(count <= desyncMaxDumpFrames);
                Debug.Assert(packetSize <= desyncMaxPayloadSize);

                if (count == 0)
                {
                    Debug.LogError("(Failed to send a desync dump!)");
                  //  network.Log("(Failed to send a desync dump!)");
                }
                else
                {
                    int startFrame = remoteNCF - (count - 1);

                   /* NetOutgoingMessage message = network.CreateMessage();
                    message.Write(latestJoinLeaveEvent); // <- consistency stream
                    message.Write(GetCurrentHostId()); // <-----'
                    message.Write(startFrame);
                    message.Write(count);*/
                    //发送信息给服务器
                    for (int i = 0; i < count; i++)
                    {
                        var buffer = snapshotBuffer[startFrame + i];
                    //    message.Write(buffer.Length);
                    //    message.Write(buffer);
                    }

                   // remotePeer.Send(message, NetDeliveryMethod.ReliableOrdered, desyncDumpChannel);


                    // Also do a local dump of that frame (while we know it exists)
                    if (dumpTarget != null)
                        dumpTarget.ExportSimpleDesyncFrame(snapshotBuffer[remoteNCF]);
                }
            }
        }




        //
        // REMOTE / INCOMING
        //



        // Why we don't need to do any buffering:
        // 
        // - We sent them a NCF with a hash
        // - They (potentially) buffered that until their own NCF caught up
        // - Once they detected a desync, they send us the dump - we are still at or past that NCF
        // (We still need to check we are on the same JLE/Host stream)
        //

        HashSet<int> receivedDesyncDebugFrom = new HashSet<int>();

        private void ReceiveDesyncDebug()
        {
            //先学写死RemoteID
            int connectionId = 1;
          //  int connectionId = remotePeer.PeerInfo.ConnectionId;

            // Security: Disallow handling of multiple dump packets from the same client
            if (!receivedDesyncDebugFrom.Add(connectionId))
                return;

            // No matter what, stop holding back clean-up for the affected frames:
            if (desyncedRemotes.ContainsKey(connectionId))
                desyncedRemotes[connectionId] = int.MaxValue;


            int receivedJLE = 1;
            int receivedHostId = 1;
            int receivedStartFrame = 1;
            int receivedCount = 1;
            byte[][] receivedSnapshots = new byte[0][];

            try
            {
                /*   receivedJLE = message.ReadInt32();
                   receivedHostId = message.ReadInt32();
                   receivedStartFrame = message.ReadInt32();
                   receivedCount = message.ReadInt32();
                   */
                receivedJLE = 1;
                receivedHostId = 1;
                receivedStartFrame = 1;
                receivedCount = 1; 
                if (receivedCount > desyncMaxDumpFrames)
                    Debug.LogError("Too many frames in desync dump");
                 //   throw new Exception("Too many frames in desync dump");
                receivedSnapshots = new byte[receivedCount][];

                for (int i = 0; i < receivedCount; i++)
                {
                    //   int length = message.ReadInt32();
                    int length = 1;
                    if (length > desyncMaxSnapshotSize)
                        Debug.LogError("Desync dump snapshot too large");
                    //  throw new Exception("Desync dump snapshot too large");
                    //  receivedSnapshots[i] = message.ReadBytes(length);
                    receivedSnapshots[i] = new byte[0];
                }
            }
            catch (Exception e) { Debug.LogError("Bad Remote Desync Snapshot"); }

         //   network.Log("Received desync dump from Peer #" + connectionId + " \"" + remotePeer.PeerInfo.PlayerName + "\"");
            Debug.Log("Received desync dump from Peer #" + connectionId );

            if (receivedJLE != latestJoinLeaveEvent || receivedHostId != GetCurrentHostId())
                return; // Different consistency stream

            byte[] previousFrameSnapshot = null;
            for (int i = 0; i < receivedCount; i++)
            {
                int frame = receivedStartFrame + i;
                if (frame > newestConsistentFrame)
                    break; // only consistent frames are comparable

                byte[] localSnapshot;
                if (snapshotBuffer.TryGetValue(frame, out localSnapshot))
                {
                    if (!RollbackBufferCompare.CompareBuffers(receivedSnapshots[i], localSnapshot))
                    {
                        // Found the desync frame!
                        if (dumpTarget != null)
                            dumpTarget.ExportComparativeDesyncDump(previousFrameSnapshot, localSnapshot, receivedSnapshots[i]);

                        return;
                    }
                }

                previousFrameSnapshot = localSnapshot;
            }
        }



        #endregion

        #region Buffer Cleanup

        int CleanUpBeforeFrame
        {
            get
            {
                int cleanUpBefore = Math.Min(newestConsistentFrame, serverNewestConsistentFrame); // TODO: local NCF is covered by global NCF, and is server necessary?
                cleanUpBefore = Math.Min(cleanUpBefore, GetGlobalMinimumNCF());

                // Buffered JLEs could be unwound at any point, so don't clean up past them:
                foreach (var jle in joinLeaveEvents)
                {
                    if (jle.frame < cleanUpBefore)
                        cleanUpBefore = jle.frame;
                }

                return cleanUpBefore;
            }
        }
        void CleanupBuffers()
        {
            // Clean up frame buffers:
            int cleanUpBefore = CleanUpBeforeFrame;

            // If this triggers, we're keeping too many buffered values!
            Debug.Assert(cleanUpBefore >= CurrentFrame - DebugBackstop);

            // Never clean up beyond the current simulation position:
            Debug.Assert(cleanUpBefore <= CurrentFrame);
            Debug.Assert(cleanUpBefore <= CurrentSimulationFrame);

            for (int i = 0; i < inputBuffers.Length; i++)
                inputBuffers[i].CleanUpBefore(cleanUpBefore);
            for (int i = 0; i < onlineStateBuffers.Length; i++)
                onlineStateBuffers[i].CleanUpBefore(cleanUpBefore);

            snapshotBuffer.CleanUpBefore(cleanUpBefore);
            hashBuffer.CleanUpBefore(cleanUpBefore);

            // Clean up JLE buffer:
            int globalMinJLE = GetGlobalMinimumJLE();
            while (joinLeaveEvents.Count > 0 && joinLeaveEvents[0].eventId <= globalMinJLE)
            {
                joinLeaveEvents.RemoveAt(0);
            }
            hostForJLE.CleanUpBefore(globalMinJLE - 1);
        }

        #endregion
        #region Remote NCFs and JLE# tracking

        /// <summary>The <see cref="newestConsistentFrame"/> on the server</summary>
        int serverNewestConsistentFrame;

        private class RemoteStatus
        {
            public int ncf, jle;
        }

        Dictionary<int, RemoteStatus> remoteStatuses = new Dictionary<int, RemoteStatus>();
        int GetGlobalMinimumNCF()
        {
            int minNCF = newestConsistentFrame;
            foreach (var rs in remoteStatuses.Values)
            {
                if (rs.ncf < minNCF)
                    minNCF = rs.ncf;
            }
            return minNCF;
        }
        int GetGlobalMinimumJLE()
        {
            int minJLE = latestJoinLeaveEvent;
            foreach (var rs in remoteStatuses.Values)
            {
                if (rs.jle < minJLE)
                    minJLE = rs.jle;
            }
            return minJLE;
        }
        void StartTrackingRemoteNCFAndJLE(int connectionId, int initialNCF, int initialJLE)
        {
            remoteStatuses.Add(connectionId, new RemoteStatus { ncf = initialNCF, jle = initialJLE });
        }
        void StopTrackingRemoteNCFAndJLE(int connectionId)
        {
            bool removed = remoteStatuses.Remove(connectionId);
            Debug.Assert(removed);
        }
        void ReconstructRemoteStatuses(int initialNCF, int initialJLE)
        {
            // Remove statuses for peers that don't exist (host migration removed them)
            if (remoteStatuses.Count > 0)
            {
                List<int> toRemove = new List<int>();
                foreach (var connectionId in remoteStatuses.Keys)
                {
                   /* foreach (var remotePeer in network.RemotePeers)
                    {
                        if (remotePeer.PeerInfo.ConnectionId == connectionId)
                            goto next;
                    }*/
                    // Not found:
                    toRemove.Add(connectionId);
                    next:
                    ;
                }

                foreach (var connectionId in toRemove)
                    remoteStatuses.Remove(connectionId);
            }

            // Add statuses for peers we aren't tracking yet:
            /*foreach (var remotePeer in network.RemotePeers)
            {
                if (!remoteStatuses.ContainsKey(remotePeer.PeerInfo.ConnectionId))
                {
                    StartTrackingRemoteNCFAndJLE(remotePeer.PeerInfo.ConnectionId, initialNCF, initialJLE);
                }
            }*/

        }
        void WriteLocalNCFAndJLE()
        {
           /* message.Write(newestConsistentFrame);
            message.Write(latestJoinLeaveEvent);
            message.Write(GetCurrentHostId());
            message.Write(GetHashForSnapshot(newestConsistentFrame));*/
        }

        void ReceiveRemoteNCFAndJLE( int relativeToFrame)
        {
            int receivedJLE = 1;
            int receivedNCF = 1;
            int receivedHostId = 1;
            uint receivedHash = 1;
            try
            {
               /* receivedNCF = message.ReadInt32();
                receivedJLE = message.ReadInt32();
                receivedHostId = message.ReadInt32();
                receivedHash = message.ReadUInt32();*/
            }
            catch (Exception e) { Debug.LogError("Bad NCF&JLE message"); }


            //int connectionId = remotePeer.PeerInfo.ConnectionId;
            int connectionId = 1;
            Debug.Assert(remoteStatuses.ContainsKey(connectionId)); // Should have been added on join
            Debug.Assert(remoteStatuses.Count <= InputAssignmentExtensions.MaxPlayerInputAssignments); // Check remotes are being removed when they leave
            RemoteStatus remoteStatus = remoteStatuses[connectionId];

            // If the remote is on a different host's JLE stream, cannot trust their JLE# and, by extension, their NCF
            if (receivedHostId != GetCurrentHostId())
                return;

          //  Debug.Assert(!network.LocalPeerInfo.IsServer || receivedJLE <= latestJoinLeaveEvent); // <- If we're the server, clients should not be getting ahead!

            if (receivedJLE == latestJoinLeaveEvent) // <- sync with join/leave (clients can move NCF backwards on join/leave)
            {
                if (remoteStatus.ncf < receivedNCF) // <- updates are received out-of-order, so skip old data
                {
                    remoteStatus.ncf = receivedNCF;

                    // At this point, the received hash is on the same branch of consistent frames (host and JLE), so we can test it
                    // Not bothering with old (out-of-order) hashes, as the relevant NCF may have been cleaned up at this point, and we'd have to test for that.
                   // ReceiveRemoteHash(receivedNCF, receivedHash, receivedJLE, receivedHostId, remotePeer.PeerInfo.ConnectionId);
                    ReceiveRemoteHash(receivedNCF, receivedHash, receivedJLE, receivedHostId,1);
                }
            }

            if (remoteStatus.jle < receivedJLE) // <- updates are received out-of-order, so skip old data
                remoteStatus.jle = receivedJLE;


            // Also keep SNCF updated
           /* if (remotePeer.PeerInfo.IsServer)
            {
                if (receivedJLE == latestJoinLeaveEvent)
                    if (serverNewestConsistentFrame < receivedNCF)
                        serverNewestConsistentFrame = receivedNCF;
            }*/
        }
        /// <summary>Clients can move their NCF back as far as the server's NCF on a join/leave event. Pair this call with an increment to <see cref="latestJoinLeaveEvent"/></summary>
        void ResetRemoteNCFs(int frame)
        {
            Debug.Assert(frame >= serverNewestConsistentFrame);
            foreach (var remoteStatus in remoteStatuses.Values)
            {
                if (remoteStatus.ncf > frame)
                    remoteStatus.ncf = frame;
            }
        }

        /// <summary>Host migrations can undo future JLEs, sending the JLE# backwards. This, in turn, will reset the NCF.</summary>
        void ResetRemoteJLEsAndNCFs(int jle, int frame)
        {
            foreach (var remoteStatus in remoteStatuses.Values)
            {
                if (remoteStatus.jle > jle)
                {
                    remoteStatus.jle = jle;

                    // TODO: I need to sit down and have a proper think about this frame resetting behaviour
                    //       (I think resetting to the SNCF at host migration time is required and will work. Need to prove it.)
                    if (remoteStatus.ncf > frame)
                        remoteStatus.ncf = frame;
                }
            }
        }
        /// <summary>Backstop remote values that block clean-up</summary>
        void CheckRemoteNCFAndJLEBackstop()
        {
            foreach (var rsEntry in remoteStatuses)
            {
                if (rsEntry.Value.ncf < CurrentFrame - RemoteNCFBackstop || rsEntry.Value.jle < latestJoinLeaveEvent - RemoteJLEBackstop)
                    HitRemoteNFCOrJLEBackstopFor(rsEntry.Key);
            }

            foreach (var jle in joinLeaveEvents)
            {
                if (jle.frame < CurrentFrame - RemoteJLEKeepFramesBackstop) // Buffered Join/Leave event is keeping too many frames buffered
                {
                    foreach (var rsEntry in remoteStatuses) // Remove those responsible for keeping the JLE buffered
                        if (rsEntry.Value.jle <= jle.eventId)
                            HitRemoteNFCOrJLEBackstopFor(rsEntry.Key);
                }
            }
        }

        void HitRemoteNFCOrJLEBackstopFor(int connectionId)
        {
           /* foreach (RemotePeer remotePeer in network.RemotePeers)
            {
                if (remotePeer.PeerInfo.ConnectionId == connectionId)
                {
                    if (remotePeer.IsConnected) // Prevent log flood
                    {
                        network.Log("Hit remote NCF/JLE backstop for " + remotePeer.PeerInfo);
                        network.NetworkDataError(remotePeer, null);
                    }
                }
            }*/
        }

        #endregion


        #region Online State Buffer Replication

        /// <summary>Write the online state buffer for a given consistent frame onwards, as well as input buffer fix-ups for any "offline" events.</summary>
        /// <remarks>Any input indices online before the consistent frame have their online state clamped to the consistent frame.</remarks>
        void WriteOnlineStateBuffer( int consistentFrame)
        {
            for (int b = 0; b < onlineStateBuffers.Length; b++)
            {
                int lastJoinFrame = -1;
                OnlineState onlineState;
                int i;

                // Find and write any join entries in the buffer before or at the consistency point
                int frame = onlineStateBuffers[b].TryGetLastBeforeOrAtFrame(consistentFrame, out onlineState, out i);
                if (i >= 0 && onlineState.Online)
                {
                    // Clamp written entries to the consistentcy point (so that written input fix-ups for leaves work)
                   /* message.Write(lastJoinFrame = consistentFrame);
                    message.Write(onlineState.JoiningPlayerName);
                    message.WriteByteArray(onlineState.JoiningPlayerData);*/
                }

                for (++i; i < onlineStateBuffers[b].Count; i++)
                {
                 //   WriteOnlineStateRecursiveHelper(b, onlineStateBuffers[b].Keys[i], onlineStateBuffers[b].Values[i], message, ref lastJoinFrame);
                    WriteOnlineStateRecursiveHelper(b, onlineStateBuffers[b].Keys[i], onlineStateBuffers[b].Values[i], ref lastJoinFrame);
                }

                // Write terminator:
                //message.Write((Int32)(-1));
            }
        }

        void WriteOnlineStateRecursiveHelper(int inputIndex, int frame, OnlineState onlineState, ref int lastJoinFrame)
        {
            if (onlineState.PreviousThisFrame != null)
                WriteOnlineStateRecursiveHelper(inputIndex, frame, onlineState.PreviousThisFrame, ref lastJoinFrame);
          //  WriteOnlineStateRecursiveHelper(inputIndex, frame, onlineState.PreviousThisFrame, message, ref lastJoinFrame);

            Debug.Assert(lastJoinFrame != -1 || onlineState.Online); // First written entry is always a join

            //message.Write(frame);
            if (onlineState.Online)
            {
              //  message.Write(onlineState.JoiningPlayerName);
              //  message.WriteByteArray(onlineState.JoiningPlayerData);
                lastJoinFrame = frame;
            }
            else
            {
                //    WriteInputRLE(inputBuffers[inputIndex], message, lastJoinFrame, frame - lastJoinFrame);
                WriteInputRLE(inputBuffers[inputIndex], lastJoinFrame, frame - lastJoinFrame);
            }
        }


        void ReceiveOnlineStateBuffer( int consistentFrame)
        {
            for (int b = 0; b < onlineStateBuffers.Length; b++)
            {
                bool join = true; // This alternates, always starting with a join
                int lastJoinFrame = -1;

                while (true)
                {
                    int frame = 0;
                    string name = null;
                    byte[] data = null;
                    try
                    {
                        //  frame = message.ReadInt32();
                        frame = 1;
                        if (frame == -1)
                            break; // Terminator

                        if (join)
                        {
                            /*  name = message.ReadString().FilterName();
                              data = message.ReadByteArray();*/
                            name = "name"; 
                            data = new byte[0]; 
                        }
                    }
                    catch (Exception e) { Debug.LogError("Bad online state buffer"); }

                    // First entry can be at the consistency point, remaining entries must be after the consistency point
                    if (lastJoinFrame == -1)
                    {
                        if (frame < consistentFrame)
                            Debug.LogError("Invalid online state buffer initial frame");
                    }
                    else
                    {
                        if (frame <= consistentFrame)
                            Debug.LogError("Invalid online state buffer frame");
                    }

                    AppendOnlineStateChange(0, b, frame, name, data);

                    if (join)
                    {
                        lastJoinFrame = frame;
                    }
                    else // leave
                    {
                        ReceiveInputRLEKnownLength(inputBuffers[b], lastJoinFrame, frame);
                    }

                    join = !join;
                }
            }
        }

        #endregion

        #region Input Receive/Write RLE

        const int rleBits = 6;
        const int rleMaxCount = (1 << rleBits) - 1;


        /// <param name="inputBuffer">The input buffer to read into, or null to ignore the read data (seek through it)</param>
        /// <param name="endFrame">The frame following the final frame written.</param>
        void ReceiveInputRLE(InputBuffer inputBuffer, int startFrame, out int endFrame)
        {
            endFrame = startFrame;

            while (true)
            {
                int count = 0;
                InputState inputState = InputState.Input0;
                try
                {
                    //   count = (int)message.ReadUInt32(rleBits);
                    count = 1;
                    if (count == 0) // Terminator
                        return;
                 //   inputState = message.ReadInputState(inputBitsUsed);
                }
                catch (Exception e) { Debug.LogError("Bad input message (RLE)");  }

                if (inputBuffer != null)
                {
                    for (int i = 0; i < count; i++)
                    {
                        ApplyInput(inputBuffer, endFrame + i, inputState);
                    }
                }

                endFrame += count;
            }
        }


        void ReceiveInputRLEKnownLength(InputBuffer inputBuffer, int startFrame, int endFrame)
        {
            int actualEndFrame;
            ReceiveInputRLE(inputBuffer, startFrame, out actualEndFrame);

            if (actualEndFrame != endFrame)
                Debug.LogError("RLE input length mismatch");
        }



        void WriteInputRLE(InputBuffer inputBuffer, int startFrame, int count)
        {
            Debug.Assert(count >= 0);

            if (count == 0) // Special case
                goto writeTerminator;

            int startIndex = inputBuffer.Keys.IndexOf(startFrame);

            // Expect to have contiguous set of inputs to write out.
            Debug.Assert(startIndex != -1);
            Debug.Assert(startIndex + count <= inputBuffer.Count); // (still might not be contiguous)

            int currentRunLength = 1;
            InputState currentInputState = inputBuffer.Values[startIndex];

            for (int i = 1; i < count; i++)
            {
                Debug.Assert(inputBuffer.Keys[startIndex + i - 1] + 1 == inputBuffer.Keys[startIndex + i]); // check input frames are contiguous
                Debug.Assert(currentRunLength > 0 && currentRunLength <= rleMaxCount); // check RLE range calculation

                InputState nextInputState = inputBuffer.Values[startIndex + i];

                if (currentRunLength == rleMaxCount || currentInputState != nextInputState)
                {
                  //  message.Write((uint)currentRunLength, rleBits);
                 //   message.WriteInputState(currentInputState, inputBitsUsed);

                    currentRunLength = 0;
                    currentInputState = nextInputState;
                }

                currentRunLength++;
            }

            Debug.Assert(currentRunLength > 0 && currentRunLength <= rleMaxCount); // check RLE range calculation
        //    message.Write((uint)currentRunLength, rleBits);
         //   message.WriteInputState(currentInputState, inputBitsUsed);

            writeTerminator:
                Debug.Log("writeTerminator");
         //   message.Write((uint)0, rleBits);
        }


        #endregion

        #region Input Buffers

        InputBuffer[] inputBuffers;

        void SetupInputBuffers()
        {
            inputBuffers = new InputBuffer[InputAssignmentExtensions.MaxPlayerInputAssignments];
            for (int i = 0; i < inputBuffers.Length; i++)
            {
                inputBuffers[i] = new InputBuffer();
            }
        }

        InputBuffer LocalInputBuffer
        {
            get
            {
                int localInputAssignment = 1;
               // int localInputAssignment = network.LocalPeerInfo.InputAssignment.GetFirstAssignedPlayerIndex();
                return inputBuffers[localInputAssignment];
            }
        }


        MultiInputState GetInputForFrame(int frame)
        {
            Debug.Assert(inputBuffers.Length == InputAssignmentExtensions.MaxPlayerInputAssignments);

            MultiInputState input = new MultiInputState();
            for (int i = 0; i < inputBuffers.Length; i++)
            {
                // If there is no input on a given frame, predict it as equal to the latest input at that frame
                input[i] = inputBuffers[i].GetLastBeforeOrAtFrameOrDefault(frame);
            }

            return input;
        }


        void ApplyInput(InputBuffer inputBuffer, int inputFrame, InputState inputState)
        {
            // NOTE: Lidgren will merrily give duplicates of messages sent as ReliableUnordered! (This is a bug in Lidgren)
            //       So we need to ignore them if they come through...

            if (inputFrame <= newestConsistentFrame) // Already have/had this frame (and it could have been cleaned up)
                return;

            // If we're getting a duplicate frame, check that it's not changing value (which would be *very* weird)
            // (This is a protocol error, but there's little that can be done about it from here - can't tell who's value is wrong!)
            Debug.Assert(!(inputBuffer.ContainsKey(inputFrame) && inputBuffer[inputFrame] != inputState));

            // Note that the following will work, even if the frame is already in the buffer (received a duplicate)
            InputState previousLogicalInputState = inputBuffer.GetLastBeforeOrAtFrameOrDefault(inputFrame);
            inputBuffer[inputFrame] = inputState;

            if (inputState != previousLogicalInputState)
                MarkPredictionDirty(inputFrame);
        }

        #endregion

        #region Prediction

        int predictionDirtyFrame = Int32.MaxValue;

        /// <summary>
        /// Indiciate that the given frame was modified since it was last predicted.
        /// Will cause the prior snapshot to be loaded as the starting point for prediction.
        /// </summary>
        void MarkPredictionDirty(int dirtyAtFrame)
        {
            Debug.Assert(dirtyAtFrame > newestConsistentFrame);
            if (dirtyAtFrame < predictionDirtyFrame)
                predictionDirtyFrame = dirtyAtFrame;
        }


        int? clientStartupSnapshotLoaded;

        void DoPrediction()
        {
            if (predictionDirtyFrame > CurrentSimulationFrame)
                return; // Nothing to predict

            Debug.Assert(predictionDirtyFrame > newestConsistentFrame);

            if (clientStartupSnapshotLoaded.HasValue)
            {
                Debug.Assert(clientStartupSnapshotLoaded.Value == predictionDirtyFrame - 1);
            }
            else
            {
                game.BeforePrediction();
                //TODO 反序列化
             //   game.Deserialize(snapshotBuffer[predictionDirtyFrame - 1]);
            }


            // Prediction loop:
            for (int frame = predictionDirtyFrame; frame <= CurrentSimulationFrame; frame++)
            {
                game.BeforeRollbackAwareFrame(frame, clientStartupSnapshotLoaded.HasValue);
                RunGameJoinLeaveEvents(frame, false);
                game.Update(GetInputForFrame(frame), false);
                game.AfterRollbackAwareFrame();

                // Save the state that we predicted (so we can reload and predict from it later)
                // TODO: Pool the snapshot objects that we are replacing here!
                //反序列化
              //  snapshotBuffer[frame] = game.Serialize();
                hashBuffer.Remove(frame);
            }


            if (clientStartupSnapshotLoaded.HasValue)
            {
                clientStartupSnapshotLoaded = null;
            }
            else
            {
                game.AfterPrediction();
            }

            // Mark prediction clean:
            predictionDirtyFrame = Int32.MaxValue;
        }

        #endregion

        #region Client Timer Synchronisation

        public const int FramesPerSecond = 60;
        public static readonly TimeSpan FrameTime = new TimeSpan(166667); // 60 FPS


        PacketTimeTracker packetTimeTracker;
        SynchronisedClock synchronisedClock;

        void ClientSetupTiming(int serverCurrentFrame)
        {
            packetTimeTracker = new PacketTimeTracker();

            // Get initial values for timing
            // TODO: We should wait until we have more timing data before starting the game running
            //   ClientReceiveTimingPacket(serverCurrentFrame, messageForTiming);
            ClientReceiveTimingPacket(serverCurrentFrame);
            //不知道是否正确
            packetTimeTracker.Update(Time.time);

            CurrentFrame = Math.Max(serverCurrentFrame, (int)Math.Round(packetTimeTracker.DesiredCurrentFrame));
            CurrentSimulationFrame = Math.Max(serverCurrentFrame, CurrentFrame - LocalFrameDelay);

            Debug.Log("Client starting time at input frame = " + CurrentFrame + ", simulation frame = " + CurrentSimulationFrame);

            synchronisedClock = new SynchronisedClock(packetTimeTracker);
        }


        void ClientReceiveTimingPacket(int remoteCurrentFrame)
        {
            //计算RTT
            // double rtt = messageForTiming.SenderConnection.AverageRoundtripTime;
            double rtt = 0;
            if (rtt < 0)
            {
                // Lidgren sets the RTT to -1 until it has enough data to initialise it. So if we get here, we don't have a RTT estimate for the server yet.
                // A well-behaved server should hold-off making us app-connected until we have told it we have its RTT with P2PClientMessage.RTTInitialised.
                // So this should normally never happen.
                //
                // BUT: During host migration, it's possible for a peer to become server who we don't have an RTT for (they freshly connected).
                // Ideally we'd like to disregard these timing values (continuing to run on our own clock for a while should be more accurate).
                // But we have to clock-sync eventually (even if it has the wrong delay). So rather than writing the complicated code to make this work,
                // just guess a value and depend on the clock-sync code to smooth it out.

                rtt = 0.05; // <- Also good if the server misbehaves in production. Don't really want RTT=-1 making it into the timer.

                // If the server was *not* a host-migration (and checking for connection ID 0 is a lazy, inaccurate way to tell),
                // then it's a programmer error (or misbehaved server):
                Debug.Assert(GetCurrentHostId() != 0);
            }
            double oneWayLatency = rtt / 2.0;

            //TODO 接收时间-单方向延时
          //  double estimatedLocalTimeOfSend = messageForTiming.ReceiveTime - oneWayLatency;
            double estimatedLocalTimeOfSend = 0 - oneWayLatency;
            packetTimeTracker.ReceivePacket(remoteCurrentFrame, estimatedLocalTimeOfSend);
        }


        void ClientUpdateTiming(TimeSpan elapsedTime)
        {
         //   Debug.Assert(network.IsApplicationConnected);
          ///  Debug.Assert(!network.IsServer);

            packetTimeTracker.Update(Time.time);
            synchronisedClock.Update(elapsedTime.TotalSeconds);
        }

        #endregion
        #region Input Broadcast

        // TODO: REMOVE ME! (this exists for testing only)
        public bool debugDisableInputBroadcast;


        int _localInputSourceIndex = 0;
        public int LocalInputSourceIndex
        {
            get { return _localInputSourceIndex; }
            set
            {
                if (value >= 0 && value < MultiInputState.Count)
                    _localInputSourceIndex = value;
            }
        }


        int coalescedInputCount;
        InputState? coalescedInput;

        void SendOrCoalesceInput(InputState inputState)
        {
            // We tolerate duplicate input frames when receiving, so the fact we could be re-sending
            // inputs that we already sent during a join doesn't matter.

            Debug.Assert(coalescedInputCount >= 0 && coalescedInputCount <= coalescedInputMaxCount);
            if (!coalescedInput.HasValue || coalescedInput.Value != inputState || coalescedInputCount == coalescedInputMaxCount)
            {
                // Send:
              /*  NetOutgoingMessage message = network.CreateMessage();
                WriteInputHeader(message, InputFormat.Coalesced, CurrentFrame - coalescedInputCount);
                WriteInputCoalesced(message, coalescedInput, coalescedInputCount, inputState);
                WriteLocalNCFAndJLE(message);
                */
                if (!debugDisableInputBroadcast)
                 //   network.Broadcast(message, NetDeliveryMethod.ReliableUnordered, 0);

                // Start new delay:
                coalescedInputCount = 0;
                coalescedInput = inputState;
            }
            else
            {
                // Continue delaying:
                coalescedInputCount++;
                coalescedInput = inputState;
            }
        }


        /// <summary>Note: This expects that the current frame is set to the frame that this input is for (this input is to advance currentFrame-1 to currentFrame)</summary>
        void ExtractAndBroadcastLocalInput(MultiInputState unnetworkedInputs)
        {
            InputState localInput = unnetworkedInputs[LocalInputSourceIndex];

            Debug.Assert(!LocalInputBuffer.ContainsKey(CurrentFrame));
            LocalInputBuffer.Add(CurrentFrame, localInput);

            SendOrCoalesceInput(localInput);
        }


        #endregion

        #region Input Receive/Write Coalesced

        const int coalescedInputBits = 4;
        const int coalescedInputMaxCount = (1 << coalescedInputBits) - 1;


        /// <param name="endFrame">The frame following the final frame written.</param>
        void ReceiveInputCoalesced(InputBuffer inputBuffer, int frame, out int endFrame)
        {
            endFrame = frame;

            int firstInputCount = 1;
            InputState firstInput = default(InputState);
            InputState lastInput = InputState.Input0;
            try
            {
                /*firstInputCount = (int)message.ReadUInt32(coalescedInputBits);
                if (firstInputCount > 0)
                    firstInput = message.ReadInputState(inputBitsUsed);
                lastInput = message.ReadInputState(inputBitsUsed);*/
            }
            catch (Exception e) { Debug.LogError("Bad input message (coalesced)"); }


            for (int i = 0; i < firstInputCount; i++)
            {
                ApplyInput(inputBuffer, endFrame, firstInput);
                endFrame++;
            }

            ApplyInput(inputBuffer, endFrame, lastInput);
            endFrame++;
        }


        void WriteInputCoalesced( InputState? firstInput, int firstInputCount, InputState lastInput)
        {
            Debug.Assert(firstInputCount >= 0 && firstInputCount <= coalescedInputMaxCount);
            Debug.Assert(firstInputCount == 0 || firstInput.HasValue);

           /* message.Write((uint)firstInputCount, coalescedInputBits);
            if (firstInputCount > 0)
                message.WriteInputState(firstInput.Value, inputBitsUsed);
            message.WriteInputState(lastInput, inputBitsUsed);*/
        }

        #endregion

        #region Input Messages

        enum InputFormat
        {
            Coalesced = 0,
            RLE = 1
        }


        /// <summary>Read an input message (with a header) into the given input buffer.</summary>
        void ReceiveInputMessage( InputBuffer inputBuffer)
        {
            bool isRLE = false;
            int frame = 1;
            try
            {
              //  isRLE = message.ReadBoolean();
             //   frame = message.ReadInt32();
            }
            catch (Exception e) { Debug.LogError("Bad input message"); }

            if (frame > CurrentFrame + InputExcessFrontstop)
            {
            //    RejectInputPastFrontstop(remotePeer);
                return;
            }

            int frameAfterRemoteCurrentFrame = 0;
            if (isRLE)
            {
             //   ReceiveInputRLE(inputBuffer, message, frame, out frameAfterRemoteCurrentFrame);
            }
            else
            {
              //  ReceiveInputCoalesced(inputBuffer, message, frame, out frameAfterRemoteCurrentFrame);
              //  ReceiveRemoteNCFAndJLE(remotePeer, frame, message);
            }

            if (frameAfterRemoteCurrentFrame - 1 > CurrentFrame + InputExcessFrontstop)
            {
           //     RejectInputPastFrontstop(remotePeer);
                return;
            }

          /*  if (remotePeer.PeerInfo.IsServer)
            {
                ClientReceiveTimingPacket(frameAfterRemoteCurrentFrame - 1, message);
            }*/
        }


        /// <summary>Callers are expected to write out remaining input message following the header</summary>
        static void WriteInputHeader( InputFormat inputFormat, int startFrame)
        {
           /* message.Write(inputFormat != InputFormat.Coalesced);
            message.Write(startFrame);*/
        }


        void RejectInputPastFrontstop()
        {
           /* if (remotePeer.IsConnected)
            {
                network.Log("Hit excess input frontstop for " + remotePeer.PeerInfo);
                network.NetworkDataError(remotePeer, null);
            }*/
        }

        #endregion


        #region Update

        void ReadAllNetworkMessages()
        {
            //读取信息
           /* foreach (var remotePeer in network.RemotePeers)
            {
                InputBuffer inputBuffer = inputBuffers[remotePeer.PeerInfo.InputAssignment.GetFirstAssignedPlayerIndex()];

                // IMPORANT NOTE: Do not access message.SenderConnection.Tag from any queued messages, as this
                //                is a network-race-condition. Best simply not access it at all in the rollback driver!
                NetIncomingMessage message;
                while ((message = remotePeer.ReadMessage()) != null)
                {
                    try
                    {
                        if (message.DeliveryMethod == NetDeliveryMethod.ReliableUnordered && message.SequenceChannel == 0) // Input channel
                        {
                            ReceiveInputMessage(remotePeer, inputBuffer, message);
                        }
                        else if (message.DeliveryMethod == NetDeliveryMethod.ReliableOrdered && message.SequenceChannel == desyncDumpChannel)
                        {
                            ReceiveDesyncDebug(remotePeer, message);
                        }
                        else
                        {
                            throw new ProtocolException("Message received on unused channel from " + remotePeer.PeerInfo);
                        }
                    }
                    catch (NetworkDataException exception)
                    {
                        network.NetworkDataError(remotePeer, exception);
                    }
                }
            }*/
        }



        /// <summary>Important: Call P2PNetwork.Update before calling this method.</summary>
        public void Update(TimeSpan elapsedTime, MultiInputState unnetworkedInputs)
        {
           /* if (!network.IsApplicationConnected)
                return; // Nothing to do!*/

            // This is a suitable proxy for "have we started running" on both client and server:
            Debug.Assert(latestJoinLeaveEvent > 0);

            try
            {
                ReadAllNetworkMessages();

                CheckRemoteNCFAndJLEBackstop();

                DoPrediction();

                UpdateNewestConsistentFrame();

                /* if (network.IsServer)
                 {
                     Tick(unnetworkedInputs);
                 }*/
                ClientUpdateTiming(elapsedTime);

                // Check we're not about to flood the network.
                // Note: This limit is still quite high. Could do fancy things like RLE so we don't send
                //       a huge number of packets when approaching this limit. But probably not worth it.
                if (CurrentFrame < synchronisedClock.CurrentFrame - InputBroadcastFloodLimit)
                {
                  //  network.Disconnect(UserVisibleStrings.GameClockOutOfSync);
                    return;
                }

                while (CurrentFrame < synchronisedClock.CurrentFrame)
                    Tick(unnetworkedInputs);

                CleanupBuffers();
            }
            catch (Exception e) { return; } // The network disconnected us
        }


        int _localFrameDelay = 0;
        /// <summary>
        /// Number of frames to delay local inputs, which reduces the number of frames of "guesses" (prediction)
        /// for remote inputs (and, hence, reduces prediction glitches). Set to zero for the most responsive local inputs.
        /// Set to a low number (eg: 2) to reduce glitching at the expence of some input latency.
        /// Set to a high number (eg: 30) to greatly reduce glitching for spectating (but is unplayable locally).
        /// </summary>
        public int LocalFrameDelay
        {
            get { return _localFrameDelay; }
            set { _localFrameDelay = Math.Max(0, value); }
        }


        /// <summary>The current frame for the game state (how the game appears to the local user).</summary>
        /// <remarks>This is the frame of the last snapshot in the snapshot buffer.</remarks>
        public int CurrentSimulationFrame { get; private set; }

        /// <summary>The current frame for input and network timing (how the game appears to the network).</summary>
        /// <remarks>This is the frame of the last input in the local input buffer.</remarks>
        public int CurrentFrame { get; private set; }



        /// <param name="displayToUser">True if this is the final tick we are going to do during the update (ie: it will draw)</param>
        void Tick(MultiInputState unnetworkedInputs)
        {
            // Advance input:
            CurrentFrame++;
            ExtractAndBroadcastLocalInput(unnetworkedInputs);

            // Advance game state:
            // (Note that we don't ever move the simulation frame backwards, we just wait to catch up)
            while (CurrentSimulationFrame < CurrentFrame - LocalFrameDelay)
            {
                CurrentSimulationFrame++;

                game.BeforeRollbackAwareFrame(CurrentSimulationFrame, false);
                RunGameJoinLeaveEvents(CurrentSimulationFrame, true);
                game.Update(GetInputForFrame(CurrentSimulationFrame), true);
                game.AfterRollbackAwareFrame();

                Debug.Assert(!snapshotBuffer.ContainsKey(CurrentSimulationFrame)); // First time adding this frame
                Debug.Assert(!hashBuffer.ContainsKey(CurrentSimulationFrame)); // First time adding this frame
                //反序列化
                //snapshotBuffer[CurrentSimulationFrame] = game.Serialize();
            }
        }


        #endregion

    }
}
