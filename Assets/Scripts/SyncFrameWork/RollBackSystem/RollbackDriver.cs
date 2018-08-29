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

        /// <summary>The current (last received) JLE. Used to syncronise NCF updates between client and server when clients join/leave. 0 if no events have been received yet.</summary>
        int latestJoinLeaveEvent;
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
            RemotePeer remotePeer = null;
            foreach (var rp in network.RemotePeers)
            {
                if (rp.PeerInfo.ConnectionId == connectionId)
                {
                    remotePeer = rp;
                    break;
                }
            }

            if (remotePeer == null)
            {
                network.Log("DESYNC! Peer #" + connectionId + " (no longer connected) desynced at frame " + remoteNCF + " with JLE " + remoteJLE);
                return;
            }

            if (!desyncedRemotes.ContainsKey(connectionId)) // <- first desync encountered
            {
                desyncedRemotes.Add(connectionId, remoteNCF);

                network.Log("DESYNC! Peer #" + connectionId + " \"" + remotePeer.PeerInfo.PlayerName + "\" desynced at frame " + remoteNCF + " with JLE " + remoteJLE);


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
                    network.Log("(Failed to send a desync dump!)");
                }
                else
                {
                    int startFrame = remoteNCF - (count - 1);

                    NetOutgoingMessage message = network.CreateMessage();
                    message.Write(latestJoinLeaveEvent); // <- consistency stream
                    message.Write(GetCurrentHostId()); // <-----'
                    message.Write(startFrame);
                    message.Write(count);
                    for (int i = 0; i < count; i++)
                    {
                        var buffer = snapshotBuffer[startFrame + i];
                        message.Write(buffer.Length);
                        message.Write(buffer);
                    }

                    remotePeer.Send(message, NetDeliveryMethod.ReliableOrdered, desyncDumpChannel);


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

        private void ReceiveDesyncDebug(RemotePeer remotePeer, NetIncomingMessage message)
        {
            int connectionId = remotePeer.PeerInfo.ConnectionId;

            // Security: Disallow handling of multiple dump packets from the same client
            if (!receivedDesyncDebugFrom.Add(connectionId))
                return;

            // No matter what, stop holding back clean-up for the affected frames:
            if (desyncedRemotes.ContainsKey(connectionId))
                desyncedRemotes[connectionId] = int.MaxValue;


            int receivedJLE;
            int receivedHostId;
            int receivedStartFrame;
            int receivedCount;
            byte[][] receivedSnapshots;

            try
            {
                receivedJLE = message.ReadInt32();
                receivedHostId = message.ReadInt32();
                receivedStartFrame = message.ReadInt32();
                receivedCount = message.ReadInt32();

                if (receivedCount > desyncMaxDumpFrames)
                    throw new Exception("Too many frames in desync dump");
                receivedSnapshots = new byte[receivedCount][];

                for (int i = 0; i < receivedCount; i++)
                {
                    int length = message.ReadInt32();
                    if (length > desyncMaxSnapshotSize)
                        throw new Exception("Desync dump snapshot too large");
                    receivedSnapshots[i] = message.ReadBytes(length);
                }
            }
            catch (Exception e) { throw new ProtocolException("Bad Remote Desync Snapshot", e); }

            network.Log("Received desync dump from Peer #" + connectionId + " \"" + remotePeer.PeerInfo.PlayerName + "\"");


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
                    if (!RollbackNative.CompareBuffers(receivedSnapshots[i], localSnapshot))
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
    }
}
